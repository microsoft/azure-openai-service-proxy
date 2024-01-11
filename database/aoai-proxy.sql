--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1
-- Dumped by pg_dump version 16.1 (Debian 16.1-1.pgdg110+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: aoai; Type: SCHEMA; Schema: -; Owner: admin
--

CREATE SCHEMA aoai;


ALTER SCHEMA aoai OWNER TO admin;

--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA aoai;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner:
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA aoai;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner:
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


--
-- Name: model_type; Type: TYPE; Schema: aoai; Owner: admin
--

CREATE TYPE aoai.model_type AS ENUM (
    'openai-chat',
    'openai-embedding',
    'openai-dalle2',
    'openai-dalle3',
    'openai-whisper',
    'openai-completion'
);


ALTER TYPE aoai.model_type OWNER TO admin;

--
-- Name: add_attendee_metric(uuid, uuid, uuid, integer); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.add_attendee_metric(p_api_key uuid, p_event_id uuid, p_catalog_id uuid, p_daily_request_cap integer) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
    estimated_count BIGINT;
BEGIN
    INSERT INTO aoai.metric(api_key, event_id, catalog_id)
    VALUES (p_api_key, p_event_id, p_catalog_id);

    SELECT COUNT(*) INTO estimated_count FROM aoai.metric WHERE api_key = p_api_key and date_stamp = CURRENT_DATE;

    IF estimated_count > p_daily_request_cap THEN
        UPDATE aoai.event_attendee SET daily_locked = true WHERE api_key = p_api_key;
		INSERT INTO aoai.event_attendee_locked(api_key) VALUES (p_api_key);
    END IF;

    RETURN estimated_count;
END;
$$;


ALTER FUNCTION aoai.add_attendee_metric(p_api_key uuid, p_event_id uuid, p_catalog_id uuid, p_daily_request_cap integer) OWNER TO admin;

--
-- Name: add_event(character varying, character varying, character varying, timestamp without time zone, timestamp without time zone, character varying, character varying, character varying, character varying, integer, integer, boolean); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.add_event(p_owner_id character varying, p_event_code character varying, p_event_markdown character varying, p_start_utc timestamp without time zone, p_end_utc timestamp without time zone, p_organizer_name character varying, p_organizer_email character varying, p_event_url character varying, p_event_url_text character varying, p_max_token_cap integer, p_daily_request_cap integer, p_active boolean) RETURNS TABLE(event_id character varying, owner_id character varying, event_code character varying, event_markdown character varying, start_utc timestamp without time zone, end_utc timestamp without time zone, organizer_name character varying, organizer_email character varying, event_url character varying, event_url_text character varying, max_token_cap integer, daily_request_cap integer, active boolean)
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_hash BYTEA;
    v_guid1 uuid := aoai.uuid_generate_v4();
    v_guid2 uuid := aoai.uuid_generate_v4();
    v_guid_string VARCHAR(128);
    v_hash_string VARCHAR(64);
    v_half1 VARCHAR(4);
    v_half2 VARCHAR(4);
    v_final_hash VARCHAR(11);
BEGIN
    v_guid_string := v_guid1::VARCHAR(36) || v_guid2::VARCHAR(36);
    v_hash := aoai.digest(v_guid_string, 'sha256');
    v_hash_string := encode(v_hash, 'hex');

    v_half1 := substring(v_hash_string, 1, 4);
    v_half2 := substring(v_hash_string, 5, 4);

    v_final_hash := v_half1 || '-' || v_half2;

	INSERT INTO aoai.event(
        event_id,
        owner_id,
        event_code,
        event_markdown,
        start_utc,
        end_utc,
        organizer_name,
        organizer_email,
        event_url,
        event_url_text,
        max_token_cap,
        daily_request_cap,
        active
    )
    VALUES (
        v_final_hash,
		p_owner_id,
		p_event_code,
		p_event_markdown,
		p_start_utc,
		p_end_utc,
		p_organizer_name,
		p_organizer_email,
		p_event_url,
		p_event_url_text,
		p_max_token_cap,
		p_daily_request_cap,
		p_active
		);

    INSERT INTO aoai.owner_event_map (
        owner_id,
        event_id,
        creator
    )
    VALUES (
        p_owner_id,
        v_final_hash,
        true
    );

	RETURN QUERY
	SELECT e.event_id, e.owner_id, e.event_code, e.event_markdown, e.start_utc, e.end_utc, e.organizer_name, e.organizer_email, e.event_url, e.event_url_text, e.max_token_cap, e.daily_request_cap, e.active
	FROM aoai.event as e WHERE e.event_id = v_final_hash;

END;
$$;


ALTER FUNCTION aoai.add_event(p_owner_id character varying, p_event_code character varying, p_event_markdown character varying, p_start_utc timestamp without time zone, p_end_utc timestamp without time zone, p_organizer_name character varying, p_organizer_email character varying, p_event_url character varying, p_event_url_text character varying, p_max_token_cap integer, p_daily_request_cap integer, p_active boolean) OWNER TO admin;

--
-- Name: add_event_attendee(character varying, character varying); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.add_event_attendee(p_user_id character varying, p_event_id character varying) RETURNS uuid
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_api_key UUID;
BEGIN
	SELECT api_key INTO v_api_key FROM aoai.event_attendee WHERE user_id = p_user_id;

    IF v_api_key IS NULL THEN
		v_api_key := aoai.uuid_generate_v4();

		INSERT INTO aoai.event_attendee(user_id, event_id, active, total_requests, api_key)
		VALUES (p_user_id, p_event_id, true, 0, v_api_key);
	END IF;

    RETURN v_api_key;
END;
$$;


ALTER FUNCTION aoai.add_event_attendee(p_user_id character varying, p_event_id character varying) OWNER TO admin;

--
-- Name: clear_daily_locked_attendee(); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.clear_daily_locked_attendee() RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    updated_keys text[];
BEGIN
    UPDATE aoai.event_attendee
    SET daily_locked = false
    WHERE api_key IN (SELECT api_key FROM aoai.event_attendee_locked)
    RETURNING api_key INTO updated_keys;

    DELETE FROM aoai.event_attendee_locked WHERE api_key = ANY(updated_keys);
END;
$$;


ALTER FUNCTION aoai.clear_daily_locked_attendee() OWNER TO admin;

--
-- Name: get_attendee_authorized(character varying, uuid); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.get_attendee_authorized(p_event_code character varying, p_api_key uuid) RETURNS TABLE(user_id character varying, event_id character varying, event_code character varying, organizer_name character varying, organizer_email character varying, event_url character varying, event_url_text character varying, max_token_cap integer, daily_request_cap integer)
    LANGUAGE plpgsql
    AS $$
DECLARE
    current_utc timestamp;
BEGIN
    current_utc := current_timestamp AT TIME ZONE 'UTC';

    RETURN QUERY
    SELECT
        EA.user_id,
        EA.event_id,
        E.event_code,
        E.organizer_name,
        E.organizer_email,
        E.event_url,
        E.event_url_text,
        E.max_token_cap,
        E.daily_request_cap
    FROM
        aoai.event E
    INNER JOIN
        aoai.event_attendee EA ON E.event_id = EA.event_id
    WHERE
        EA.api_key = p_api_key AND
        EA.active = true AND
        EA.daily_locked = false AND
        E.event_code = p_event_code AND
        E.active = true AND
        current_utc BETWEEN E.start_utc AND E.end_utc;
END;
$$;


ALTER FUNCTION aoai.get_attendee_authorized(p_event_code character varying, p_api_key uuid) OWNER TO admin;

--
-- Name: get_models_by_deployment_name(character varying, character varying); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.get_models_by_deployment_name(p_event_id character varying, p_deployment_id character varying) RETURNS TABLE(deployment_name character varying, resource_name character varying, endpoint_key character varying, model_type aoai.model_type, catalog_id uuid)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
        OC.deployment_name,
        OC.resource_name,
        OC.endpoint_key,
        OC.model_type,
        OC.catalog_id
    FROM
        aoai.event_catalog_map EC
    INNER JOIN
        aoai.owner_catalog OC ON EC.catalog_id = OC.catalog_id
    WHERE
        EC.event_id = p_event_id AND
        OC.deployment_name = p_deployment_id AND
        OC.active = true;
END;
$$;


ALTER FUNCTION aoai.get_models_by_deployment_name(p_event_id character varying, p_deployment_id character varying) OWNER TO admin;

--
-- Name: get_models_by_event(character varying); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.get_models_by_event(p_event_id character varying) RETURNS TABLE(deployment_name character varying, resource_name character varying, endpoint_key character varying, model_type aoai.model_type, catalog_id uuid)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
        OC.deployment_name,
        OC.resource_name,
        OC.endpoint_key,
        OC.model_type,
        OC.catalog_id
    FROM
        aoai.event_catalog_map EC
    INNER JOIN
        aoai.owner_catalog OC ON EC.catalog_id = OC.catalog_id
    WHERE
        EC.event_id = p_event_id AND
        OC.active = true
	ORDER BY OC.deployment_name;

END;
$$;


ALTER FUNCTION aoai.get_models_by_event(p_event_id character varying) OWNER TO admin;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: event; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.event (
    event_id character varying(50) DEFAULT gen_random_uuid() NOT NULL,
    owner_id character varying(128),
    event_code character varying(64) NOT NULL,
    event_markdown character varying(8192) NOT NULL,
    start_utc timestamp(6) without time zone NOT NULL,
    end_utc timestamp(6) without time zone NOT NULL,
    organizer_name character varying(128) NOT NULL,
    organizer_email character varying(128) NOT NULL,
    event_url character varying(256) NOT NULL,
    event_url_text character varying(256) NOT NULL,
    max_token_cap integer NOT NULL,
    daily_request_cap integer NOT NULL,
    active boolean NOT NULL
);


ALTER TABLE aoai.event OWNER TO admin;

--
-- Name: event_attendee; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.event_attendee (
    user_id character varying(128) NOT NULL,
    event_id character varying(50) NOT NULL,
    active boolean NOT NULL,
    api_key uuid NOT NULL,
    daily_locked boolean DEFAULT false NOT NULL
);


ALTER TABLE aoai.event_attendee OWNER TO admin;

--
-- Name: event_attendee_locked; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.event_attendee_locked (
    api_key uuid NOT NULL
);


ALTER TABLE aoai.event_attendee_locked OWNER TO admin;

--
-- Name: event_catalog_map; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.event_catalog_map (
    event_id character varying(50) NOT NULL,
    catalog_id uuid NOT NULL
);


ALTER TABLE aoai.event_catalog_map OWNER TO admin;

--
-- Name: metric; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.metric (
    event_id character varying(50) NOT NULL,
    api_key uuid NOT NULL,
    date_stamp date DEFAULT CURRENT_DATE NOT NULL,
    request_id integer NOT NULL,
    time_stamp time without time zone DEFAULT CURRENT_TIME NOT NULL,
    catalog_id uuid NOT NULL
);


ALTER TABLE aoai.metric OWNER TO admin;

--
-- Name: owner; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.owner (
    owner_id character varying(128) NOT NULL,
    name character varying(128) NOT NULL,
    email character varying(128) NOT NULL
);


ALTER TABLE aoai.owner OWNER TO admin;

--
-- Name: owner_catalog; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.owner_catalog (
    owner_id character varying(128) NOT NULL,
    catalog_id uuid DEFAULT gen_random_uuid() NOT NULL,
    deployment_name character varying(64) NOT NULL,
    resource_name character varying(64) NOT NULL,
    endpoint_key character varying(128) NOT NULL,
    active boolean NOT NULL,
    model_type aoai.model_type NOT NULL
);


ALTER TABLE aoai.owner_catalog OWNER TO admin;

--
-- Name: owner_event_map; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.owner_event_map (
    owner_id character varying(128) NOT NULL,
    event_id character varying(50) NOT NULL,
    creator boolean NOT NULL
);


ALTER TABLE aoai.owner_event_map OWNER TO admin;

--
-- Name: request_id_seq; Type: SEQUENCE; Schema: aoai; Owner: admin
--

CREATE SEQUENCE aoai.request_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE aoai.request_id_seq OWNER TO admin;

--
-- Name: request_id_seq; Type: SEQUENCE OWNED BY; Schema: aoai; Owner: admin
--

ALTER SEQUENCE aoai.request_id_seq OWNED BY aoai.metric.request_id;


--
-- Name: metric request_id; Type: DEFAULT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.metric ALTER COLUMN request_id SET DEFAULT nextval('aoai.request_id_seq'::regclass);


--
-- Name: event event_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.event
    ADD CONSTRAINT event_pkey PRIMARY KEY (event_id);


--
-- Name: event_attendee eventattendee_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.event_attendee
    ADD CONSTRAINT eventattendee_pkey PRIMARY KEY (user_id, event_id);


--
-- Name: event_catalog_map eventcatalogmap_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.event_catalog_map
    ADD CONSTRAINT eventcatalogmap_pkey PRIMARY KEY (event_id, catalog_id);


--
-- Name: metric metric_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.metric
    ADD CONSTRAINT metric_pkey PRIMARY KEY (api_key, date_stamp, request_id);


--
-- Name: owner owner_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.owner
    ADD CONSTRAINT owner_pkey PRIMARY KEY (owner_id);


--
-- Name: owner_catalog ownercatalog_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.owner_catalog
    ADD CONSTRAINT ownercatalog_pkey PRIMARY KEY (catalog_id);


--
-- Name: owner_event_map ownereventmap_pkey; Type: CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.owner_event_map
    ADD CONSTRAINT ownereventmap_pkey PRIMARY KEY (owner_id, event_id);


--
-- Name: api_key_unique_index; Type: INDEX; Schema: aoai; Owner: admin
--

CREATE UNIQUE INDEX api_key_unique_index ON aoai.event_attendee USING btree (api_key);


--
-- Name: event_id_index; Type: INDEX; Schema: aoai; Owner: admin
--

CREATE INDEX event_id_index ON aoai.metric USING btree (event_id);


--
-- Name: event_attendee fk_eventattendee_event; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.event_attendee
    ADD CONSTRAINT fk_eventattendee_event FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: event_catalog_map fk_eventcatalogmap_event; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.event_catalog_map
    ADD CONSTRAINT fk_eventcatalogmap_event FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: event_catalog_map fk_eventcatalogmap_ownercatalog; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.event_catalog_map
    ADD CONSTRAINT fk_eventcatalogmap_ownercatalog FOREIGN KEY (catalog_id) REFERENCES aoai.owner_catalog(catalog_id) ON DELETE CASCADE;


--
-- Name: owner_catalog fk_groupmodels_group; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.owner_catalog
    ADD CONSTRAINT fk_groupmodels_group FOREIGN KEY (owner_id) REFERENCES aoai.owner(owner_id) ON DELETE CASCADE;


--
-- Name: metric fk_metric; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.metric
    ADD CONSTRAINT fk_metric FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: owner_event_map fk_ownereventmap_event; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.owner_event_map
    ADD CONSTRAINT fk_ownereventmap_event FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: owner_event_map fk_ownereventmap_owner; Type: FK CONSTRAINT; Schema: aoai; Owner: admin
--

ALTER TABLE ONLY aoai.owner_event_map
    ADD CONSTRAINT fk_ownereventmap_owner FOREIGN KEY (owner_id) REFERENCES aoai.owner(owner_id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


DROP SCHEMA IF EXISTS PUBLIC CASCADE ;
DROP DATABASE IF EXISTS postgres WITH (FORCE);
