--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1
-- Dumped by pg_dump version 16.2 (Debian 16.2-1.pgdg110+2)

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
-- Name: aoai; Type: SCHEMA; Schema: -; Owner: azure_pg_admin
--

CREATE SCHEMA aoai;


ALTER SCHEMA aoai OWNER TO azure_pg_admin;

--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA aoai;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner:
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- Name: model_type; Type: TYPE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TYPE aoai.model_type AS ENUM (
    'openai-chat',
    'openai-embedding',
    'openai-dalle3',
    'openai-whisper',
    'openai-completion',
    'openai-instruct',
    'azure-ai-search'
);


ALTER TYPE aoai.model_type OWNER TO azure_pg_admin;

--
-- Name: add_attendee_metric(character varying, character varying, uuid, jsonb); Type: PROCEDURE; Schema: aoai; Owner: azure_pg_admin
--

CREATE PROCEDURE aoai.add_attendee_metric(IN p_api_key character varying, IN p_event_id character varying, IN p_catalog_id uuid, IN p_usage jsonb)
    LANGUAGE plpgsql
    AS $$
DECLARE
	v_resource_string VARCHAR(64);
	v_token_count INTEGER;
BEGIN
--     PERFORM request_count FROM aoai.event_attendee_request
--     WHERE api_key = p_api_key AND date_stamp = CURRENT_DATE;

	v_token_count = COALESCE((p_usage ->> 'total_tokens')::integer, 0);

    IF EXISTS
		(SELECT 1 FROM aoai.event_attendee_request WHERE api_key = p_api_key AND date_stamp = CURRENT_DATE)
	THEN
        -- If a record exists, increment the count
        UPDATE aoai.event_attendee_request
        SET request_count = request_count + 1, token_count = token_count + v_token_count
        WHERE api_key = p_api_key AND date_stamp = CURRENT_DATE;
    ELSE
        -- If no record exists, insert a new one with count set to 1
        INSERT INTO aoai.event_attendee_request(api_key, date_stamp, request_count, token_count)
        VALUES (p_api_key, CURRENT_DATE, 1, v_token_count);
    END IF;

    SELECT model_type || ' | ' || deployment_name INTO v_resource_string FROM aoai.owner_catalog as oc WHERE oc.catalog_id = p_catalog_id;

    INSERT INTO aoai.metric(api_key, event_id, resource, usage)
    VALUES (p_api_key, p_event_id, v_resource_string, p_usage);
END;
$$;


ALTER PROCEDURE aoai.add_attendee_metric(IN p_api_key character varying, IN p_event_id character varying, IN p_catalog_id uuid, IN p_usage jsonb) OWNER TO azure_pg_admin;

--
-- Name: add_event(character varying, character varying, character varying, character varying, timestamp without time zone, timestamp without time zone, integer, character varying, character varying, character varying, integer, integer, boolean, character varying); Type: FUNCTION; Schema: aoai; Owner: azure_pg_admin
--

CREATE FUNCTION aoai.add_event(p_owner_id character varying, p_event_code character varying, p_event_shared_code character varying, p_event_markdown character varying, p_start_timestamp timestamp without time zone, p_end_timestamp timestamp without time zone, p_time_zone_offset integer, p_time_zone_label character varying, p_organizer_name character varying, p_organizer_email character varying, p_max_token_cap integer, p_daily_request_cap integer, p_active boolean, p_event_image_url character varying DEFAULT NULL::character varying) RETURNS TABLE(event_id character varying, owner_id character varying, event_code character varying, event_shared_code character varying, event_markdown character varying, start_timestamp timestamp without time zone, end_timestamp timestamp without time zone, organizer_name character varying, organizer_email character varying, event_image_url character varying, max_token_cap integer, daily_request_cap integer, active boolean)
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_hash BYTEA;
    v_guid1 uuid := aoai.gen_random_uuid();
    v_guid2 uuid := aoai.gen_random_uuid();
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
		event_shared_code,
        event_markdown,
        start_timestamp,
        end_timestamp,
		time_zone_offset,
		time_zone_label,
        organizer_name,
        organizer_email,
		event_image_url,
        max_token_cap,
        daily_request_cap,
        active
    )
    VALUES (
        v_final_hash,
		p_owner_id,
		p_event_code,
		p_event_shared_code,
		p_event_markdown,
		p_start_timestamp,
		p_end_timestamp,
		p_time_zone_offset,
		p_time_zone_label,
		p_organizer_name,
		p_organizer_email,
		p_event_image_url,
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
	SELECT e.event_id, e.owner_id, e.event_code, e.event_shared_code, e.event_markdown, e.start_timestamp, e.end_timestamp, e.organizer_name, e.organizer_email, e.event_image_url, e.max_token_cap, e.daily_request_cap, e.active
	FROM aoai.event as e WHERE e.event_id = v_final_hash;

END;
$$;


ALTER FUNCTION aoai.add_event(p_owner_id character varying, p_event_code character varying, p_event_shared_code character varying, p_event_markdown character varying, p_start_timestamp timestamp without time zone, p_end_timestamp timestamp without time zone, p_time_zone_offset integer, p_time_zone_label character varying, p_organizer_name character varying, p_organizer_email character varying, p_max_token_cap integer, p_daily_request_cap integer, p_active boolean, p_event_image_url character varying) OWNER TO azure_pg_admin;

--
-- Name: add_event_attendee(character varying, character varying); Type: FUNCTION; Schema: aoai; Owner: azure_pg_admin
--

CREATE FUNCTION aoai.add_event_attendee(p_user_id character varying, p_event_id character varying) RETURNS uuid
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_api_key character varying;
BEGIN
	SELECT api_key INTO v_api_key FROM aoai.event_attendee WHERE user_id = p_user_id AND event_id = p_event_id;

    IF v_api_key IS NULL THEN
		v_api_key := aoai.gen_random_uuid();

		INSERT INTO aoai.event_attendee(user_id, event_id, active, api_key)
		VALUES (p_user_id, p_event_id, true, v_api_key);
	END IF;

    RETURN v_api_key;
END;
$$;


ALTER FUNCTION aoai.add_event_attendee(p_user_id character varying, p_event_id character varying) OWNER TO azure_pg_admin;

--
-- Name: get_attendee_authorized(character varying); Type: FUNCTION; Schema: aoai; Owner: azure_pg_admin
--

CREATE FUNCTION aoai.get_attendee_authorized(p_api_key character varying) RETURNS TABLE(api_key character varying, user_id character varying, event_id character varying, event_code character varying, organizer_name character varying, organizer_email character varying, event_image_url character varying, max_token_cap integer, daily_request_cap integer, rate_limit_exceed boolean)
    LANGUAGE plpgsql
    AS $$
DECLARE
    current_utc timestamp;
	v_request_count integer;
    v_event_id character varying;
    v_event_shared_code character varying;
    v_hash BYTEA;
    v_hash_string VARCHAR(64);
    v_api_key VARCHAR(36);
BEGIN
    current_utc := current_timestamp AT TIME ZONE 'UTC';

-- 	Get the current number of requests made for current date by api_key
	SELECT COALESCE(
	    (SELECT request_count FROM aoai.event_attendee_request EAR
	    WHERE EAR.api_key = p_api_key AND date_stamp = CURRENT_DATE), 0)
		INTO v_request_count;

--  Check for shared code request in the format of event-id@event-shared-code/email-address
--  Regex explained
--  ^ asserts the start of the line.
--  [a-zA-Z0-9-]{9} matches exactly 9 characters that can be any lowercase letter (a-z), uppercase letter (A-Z), digit (0-9), or hyphen (-).
--  @{1} matches exactly one "@" character.
--  [a-zA-Z0-9]{5,} matches at least 5 characters that can be any lowercase letter (a-z), uppercase letter (A-Z), or digit (0-9).
--  / matches exactly one "/" character.
--  .{8,} matches at least 8 of any character except newline.
--  $ asserts the end of the line.

	IF p_api_key ~ '^[a-zA-Z0-9-]{9}@{1}[a-zA-Z0-9]{5,}/.{8,}$' THEN

		v_event_id := substring(p_api_key from '([a-zA-Z0-9-]+)');
		v_event_shared_code := substring(p_api_key from '@([a-zA-Z0-9]+)');

		v_hash := aoai.digest(p_api_key, 'sha256');
        v_hash_string := encode(v_hash, 'hex');

-- Create a UUID from the v_hash_string. v_hash_string is 64 chars create a uuid from chars across the hash string
		v_api_key :=
			substring(v_hash_string, 1, 8) || '-' ||
			substring(v_hash_string, 10, 4) || '-' ||
			substring(v_hash_string, 20, 4) || '-' ||
			substring(v_hash_string, 30, 4) || '-' ||
			substring(v_hash_string, 40, 12);

--  Create an attendee record if this is a shared id event request
		INSERT INTO aoai.event_attendee(user_id, event_id, active, api_key)
		SELECT 'shared:' || v_hash_string, v_event_id, true, v_api_key
		WHERE NOT EXISTS (
		    SELECT 1 FROM aoai.event_attendee EA
			WHERE EA.event_id = v_event_id and EA.api_key = v_api_key
		);

		RETURN QUERY
	    SELECT
			EA.api_key,
	        EA.user_id,
	        EA.event_id,
	        E.event_code,
	        E.organizer_name,
	        E.organizer_email,
			E.event_image_url,
	        E.max_token_cap,
	        E.daily_request_cap,
			(CASE WHEN v_request_count > E.daily_request_cap THEN true ELSE false END) AS rate_limit_exceed
	    FROM
	        aoai.event E
	    INNER JOIN
	        aoai.event_attendee EA ON E.event_id = EA.event_id
	    WHERE
	        EA.api_key = v_api_key AND
			E.event_shared_code = v_event_shared_code AND
	        EA.active = true AND
	        E.active = true AND
	        current_utc + interval '1 minute' * E.time_zone_offset between E.start_timestamp AND E.end_timestamp;

	END IF;

    RETURN QUERY
    SELECT
		EA.api_key,
        EA.user_id,
        EA.event_id,
        E.event_code,
        E.organizer_name,
        E.organizer_email,
		E.event_image_url,
        E.max_token_cap,
        E.daily_request_cap,
		(CASE WHEN v_request_count > E.daily_request_cap THEN true ELSE false END) AS rate_limit_exceed
    FROM
        aoai.event E
    INNER JOIN
        aoai.event_attendee EA ON E.event_id = EA.event_id
    WHERE
        EA.api_key = p_api_key AND
        EA.active = true AND
        E.active = true AND
        current_utc + interval '1 minute' * E.time_zone_offset between E.start_timestamp AND E.end_timestamp;
END;
$$;


ALTER FUNCTION aoai.get_attendee_authorized(p_api_key character varying) OWNER TO azure_pg_admin;

--
-- Name: get_event_registration_by_event_id(character varying); Type: FUNCTION; Schema: aoai; Owner: azure_pg_admin
--

CREATE FUNCTION aoai.get_event_registration_by_event_id(p_event_id character varying) RETURNS TABLE(event_id character varying, event_code character varying, event_image_url character varying, organizer_name character varying, organizer_email character varying, event_markdown character varying, start_timestamp timestamp without time zone, end_timestamp timestamp without time zone, time_zone_offset integer, time_zone_label character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
		e.event_id,
        e.event_code,
		e.event_image_url,
        e.organizer_name,
        e.organizer_email,
        e.event_markdown,
        e.start_timestamp,
        e.end_timestamp,
		e.time_zone_offset,
		e.time_zone_label
    FROM aoai.event as e
    WHERE e.event_id = p_event_id;
END;
$$;


ALTER FUNCTION aoai.get_event_registration_by_event_id(p_event_id character varying) OWNER TO azure_pg_admin;

--
-- Name: get_models_by_deployment_name(character varying, character varying); Type: FUNCTION; Schema: aoai; Owner: azure_pg_admin
--

CREATE FUNCTION aoai.get_models_by_deployment_name(p_event_id character varying, p_deployment_id character varying) RETURNS TABLE(deployment_name character varying, endpoint_url character varying, endpoint_key character varying, model_type aoai.model_type, catalog_id uuid, location character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
        OC.deployment_name,
        OC.endpoint_url,
        OC.endpoint_key,
        OC.model_type,
        OC.catalog_id,
		OC.location
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


ALTER FUNCTION aoai.get_models_by_deployment_name(p_event_id character varying, p_deployment_id character varying) OWNER TO azure_pg_admin;

--
-- Name: get_models_by_event(character varying); Type: FUNCTION; Schema: aoai; Owner: azure_pg_admin
--

CREATE FUNCTION aoai.get_models_by_event(p_event_id character varying) RETURNS TABLE(deployment_name character varying, endpoint_url character varying, endpoint_key character varying, model_type aoai.model_type, catalog_id uuid, location character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
        OC.deployment_name,
        OC.endpoint_url,
        OC.endpoint_key,
        OC.model_type,
        OC.catalog_id,
		OC.location
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


ALTER FUNCTION aoai.get_models_by_event(p_event_id character varying) OWNER TO azure_pg_admin;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: event; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.event (
    event_id character varying(50) DEFAULT gen_random_uuid() NOT NULL,
    owner_id character varying(128) NOT NULL,
    event_code character varying(64) NOT NULL,
    event_markdown character varying(8192) NOT NULL,
    start_timestamp timestamp(6) without time zone NOT NULL,
    end_timestamp timestamp(6) without time zone NOT NULL,
    organizer_name character varying(128) NOT NULL,
    organizer_email character varying(128) NOT NULL,
    max_token_cap integer NOT NULL,
    daily_request_cap integer NOT NULL,
    active boolean NOT NULL,
    event_image_url character varying(256),
    time_zone_offset integer NOT NULL,
    time_zone_label character varying(64) NOT NULL,
    event_shared_code character varying(64)
);


ALTER TABLE aoai.event OWNER TO azure_pg_admin;

--
-- Name: event_attendee; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.event_attendee (
    user_id character varying(128) NOT NULL,
    event_id character varying(50) NOT NULL,
    active boolean NOT NULL,
    api_key character varying(36) NOT NULL
);


ALTER TABLE aoai.event_attendee OWNER TO azure_pg_admin;

--
-- Name: event_attendee_request; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.event_attendee_request (
    api_key character varying NOT NULL,
    date_stamp date NOT NULL,
    request_count integer NOT NULL,
    token_count integer NOT NULL
);


ALTER TABLE aoai.event_attendee_request OWNER TO azure_pg_admin;

--
-- Name: event_catalog_map; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.event_catalog_map (
    event_id character varying(50) NOT NULL,
    catalog_id uuid NOT NULL
);


ALTER TABLE aoai.event_catalog_map OWNER TO azure_pg_admin;

--
-- Name: metric; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.metric (
    event_id character varying(50) NOT NULL,
    api_key character varying NOT NULL,
    date_stamp date DEFAULT CURRENT_DATE NOT NULL,
    time_stamp time without time zone DEFAULT CURRENT_TIME NOT NULL,
    resource character varying(64) NOT NULL,
    usage jsonb NOT NULL
);


ALTER TABLE aoai.metric OWNER TO azure_pg_admin;

--
-- Name: owner; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.owner (
    owner_id character varying(128) NOT NULL,
    name character varying(128) NOT NULL,
    email character varying(128) NOT NULL
);


ALTER TABLE aoai.owner OWNER TO azure_pg_admin;

--
-- Name: owner_catalog; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.owner_catalog (
    owner_id character varying(128) NOT NULL,
    catalog_id uuid DEFAULT gen_random_uuid() NOT NULL,
    deployment_name character varying(64) NOT NULL,
    endpoint_url character varying(256) NOT NULL,
    endpoint_key character varying(128) NOT NULL,
    active boolean NOT NULL,
    model_type aoai.model_type NOT NULL,
    location character varying(64) DEFAULT ''::character varying NOT NULL,
    friendly_name character varying(64) NOT NULL
);


ALTER TABLE aoai.owner_catalog OWNER TO azure_pg_admin;

--
-- Name: owner_event_map; Type: TABLE; Schema: aoai; Owner: azure_pg_admin
--

CREATE TABLE aoai.owner_event_map (
    owner_id character varying(128) NOT NULL,
    event_id character varying(50) NOT NULL,
    creator boolean NOT NULL
);


ALTER TABLE aoai.owner_event_map OWNER TO azure_pg_admin;

--
-- Name: event event_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event
    ADD CONSTRAINT event_pkey PRIMARY KEY (event_id);


--
-- Name: event_attendee eventattendee_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_attendee
    ADD CONSTRAINT eventattendee_pkey PRIMARY KEY (user_id, event_id);


--
-- Name: event_attendee_request eventattendeerequest_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_attendee_request
    ADD CONSTRAINT eventattendeerequest_pkey PRIMARY KEY (api_key, date_stamp);


--
-- Name: event_catalog_map eventcatalogmap_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_catalog_map
    ADD CONSTRAINT eventcatalogmap_pkey PRIMARY KEY (event_id, catalog_id);


--
-- Name: owner owner_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.owner
    ADD CONSTRAINT owner_pkey PRIMARY KEY (owner_id);


--
-- Name: owner_catalog ownercatalog_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.owner_catalog
    ADD CONSTRAINT ownercatalog_pkey PRIMARY KEY (catalog_id);


--
-- Name: owner_event_map ownereventmap_pkey; Type: CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.owner_event_map
    ADD CONSTRAINT ownereventmap_pkey PRIMARY KEY (owner_id, event_id);


--
-- Name: api_key_unique_index; Type: INDEX; Schema: aoai; Owner: azure_pg_admin
--

CREATE UNIQUE INDEX api_key_unique_index ON aoai.event_attendee USING btree (api_key);


--
-- Name: event_id_index; Type: INDEX; Schema: aoai; Owner: azure_pg_admin
--

CREATE INDEX event_id_index ON aoai.metric USING btree (event_id);


--
-- Name: event_attendee fk_eventattendee_event; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_attendee
    ADD CONSTRAINT fk_eventattendee_event FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: event_attendee_request fk_eventattendeerequest_eventattendee; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_attendee_request
    ADD CONSTRAINT fk_eventattendeerequest_eventattendee FOREIGN KEY (api_key) REFERENCES aoai.event_attendee(api_key) ON DELETE CASCADE;


--
-- Name: event_catalog_map fk_eventcatalogmap_event; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_catalog_map
    ADD CONSTRAINT fk_eventcatalogmap_event FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: event_catalog_map fk_eventcatalogmap_ownercatalog; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.event_catalog_map
    ADD CONSTRAINT fk_eventcatalogmap_ownercatalog FOREIGN KEY (catalog_id) REFERENCES aoai.owner_catalog(catalog_id) ON DELETE CASCADE;


--
-- Name: owner_catalog fk_groupmodels_group; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.owner_catalog
    ADD CONSTRAINT fk_groupmodels_group FOREIGN KEY (owner_id) REFERENCES aoai.owner(owner_id) ON DELETE CASCADE;


--
-- Name: metric fk_metric; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.metric
    ADD CONSTRAINT fk_metric FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: owner_event_map fk_ownereventmap_event; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.owner_event_map
    ADD CONSTRAINT fk_ownereventmap_event FOREIGN KEY (event_id) REFERENCES aoai.event(event_id) ON DELETE CASCADE;


--
-- Name: owner_event_map fk_ownereventmap_owner; Type: FK CONSTRAINT; Schema: aoai; Owner: azure_pg_admin
--

ALTER TABLE ONLY aoai.owner_event_map
    ADD CONSTRAINT fk_ownereventmap_owner FOREIGN KEY (owner_id) REFERENCES aoai.owner(owner_id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


DROP SCHEMA IF EXISTS PUBLIC CASCADE ;
