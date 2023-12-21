--
-- PostgreSQL database dump
--

-- Dumped from database version 14.10 (Ubuntu 14.10-0ubuntu0.22.04.1)
-- Dumped by pg_dump version 14.10 (Ubuntu 14.10-0ubuntu0.22.04.1)

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

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner:
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner:
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


--
-- Name: add_event(uuid, character varying, character varying, timestamp without time zone, timestamp without time zone, character varying, character varying, character varying, character varying, integer, boolean, integer, boolean); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.add_event(p_owner_id uuid, p_event_code character varying, p_event_markdown character varying, p_start_utc timestamp without time zone, p_end_utc timestamp without time zone, p_organizer_name character varying, p_organizer_email character varying, p_event_url character varying, p_event_url_text character varying, p_max_token_cap integer, p_single_code boolean, p_daily_request_cap integer, p_active boolean) RETURNS TABLE(event_id character varying, owner_id uuid, event_code character varying, event_markdown character varying, start_utc timestamp without time zone, end_utc timestamp without time zone, organizer_name character varying, organizer_email character varying, event_url character varying, event_url_text character varying, max_token_cap integer, single_code boolean, daily_request_cap integer, active boolean)
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_hash BYTEA;
    v_guid1 uuid := uuid_generate_v4();
    v_guid2 uuid := uuid_generate_v4();
    v_guid_string VARCHAR(128);
    v_hash_string VARCHAR(64);
    v_half1 VARCHAR(4);
    v_half2 VARCHAR(4);
    v_final_hash VARCHAR(11);
BEGIN
    v_guid_string := v_guid1::VARCHAR(36) || v_guid2::VARCHAR(36);
    v_hash := digest(v_guid_string, 'sha256');
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
        event_urltext,
        max_token_cap,
        single_code,
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
		p_single_code,
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
	SELECT e.event_id, e.owner_id, e.event_code, e.event_markdown, e.start_utc, e.end_utc, e.organizer_name, e.organizer_email, e.event_url, e.event_urltext, e.max_token_cap, e.single_code, e.daily_request_cap, e.active
	FROM aoai.event as e WHERE e.event_id = v_final_hash;

END;
$$;


ALTER FUNCTION aoai.add_event(p_owner_id uuid, p_event_code character varying, p_event_markdown character varying, p_start_utc timestamp without time zone, p_end_utc timestamp without time zone, p_organizer_name character varying, p_organizer_email character varying, p_event_url character varying, p_event_url_text character varying, p_max_token_cap integer, p_single_code boolean, p_daily_request_cap integer, p_active boolean) OWNER TO admin;

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
		E.event_code = p_event_code AND
        E.active = true AND
        current_utc BETWEEN E.start_utc AND E.end_utc;
END;
$$;


ALTER FUNCTION aoai.get_attendee_authorized(p_event_code character varying, p_api_key uuid) OWNER TO admin;

--
-- Name: get_models_by_deployment_name(character varying, character varying); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.get_models_by_deployment_name(p_event_id character varying, p_deployment_id character varying) RETURNS TABLE(deployment_name character varying, model_name character varying, resource_name character varying, endpoint_key character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
        OC.deployment_name,
		OC.model_name,
        OC.resource_name,
        OC.endpoint_key
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

CREATE FUNCTION aoai.get_models_by_event(p_event_id character varying) RETURNS TABLE(deployment_name character varying, model_name character varying, resource_name character varying, endpoint_key character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT
        OC.deployment_name,
		OC.model_name,
        OC.resource_name,
        OC.endpoint_key
    FROM
        aoai.event_catalog_map EC
    INNER JOIN
        aoai.owner_catalog OC ON EC.catalog_id = OC.catalog_id
    WHERE
        EC.event_id = p_event_id AND
        OC.active = true
	ORDER BY OC.model_name;

END;
$$;


ALTER FUNCTION aoai.get_models_by_event(p_event_id character varying) OWNER TO admin;

--
-- Name: get_owner_id(character varying); Type: FUNCTION; Schema: aoai; Owner: admin
--

CREATE FUNCTION aoai.get_owner_id(p_entra_id character varying) RETURNS uuid
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_owner_id UUID;
BEGIN
    SELECT owner_id INTO v_owner_id FROM aoai.owner WHERE entra_id = p_entra_id;

    IF v_owner_id IS NULL THEN
        v_owner_id := uuid_generate_v4();

        INSERT INTO aoai.owner(entra_id, owner_id)
        VALUES (p_entra_id, v_owner_id);
    END IF;

    RETURN v_owner_id;
END;
$$;


ALTER FUNCTION aoai.get_owner_id(p_entra_id character varying) OWNER TO admin;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: event; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.event (
    event_id character varying(50) DEFAULT gen_random_uuid() NOT NULL,
    owner_id uuid DEFAULT gen_random_uuid() NOT NULL,
    event_code character varying(64) NOT NULL,
    event_markdown character varying(8192) NOT NULL,
    start_utc timestamp(6) without time zone NOT NULL,
    end_utc timestamp(6) without time zone NOT NULL,
    organizer_name character varying(128) NOT NULL,
    organizer_email character varying(128) NOT NULL,
    event_url character varying(256) NOT NULL,
    event_url_text character varying(256) NOT NULL,
    max_token_cap integer NOT NULL,
    single_code boolean NOT NULL,
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
    total_requests integer NOT NULL,
    api_key uuid NOT NULL,
    total_tokens integer
);


ALTER TABLE aoai.event_attendee OWNER TO admin;

--
-- Name: event_catalog_map; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.event_catalog_map (
    event_id character varying(50) NOT NULL,
    catalog_id uuid NOT NULL
);


ALTER TABLE aoai.event_catalog_map OWNER TO admin;

--
-- Name: owner; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.owner (
    entra_id character varying(128) NOT NULL,
    owner_id uuid DEFAULT gen_random_uuid() NOT NULL
);


ALTER TABLE aoai.owner OWNER TO admin;

--
-- Name: owner_catalog; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.owner_catalog (
    owner_id uuid NOT NULL,
    catalog_id uuid DEFAULT gen_random_uuid() NOT NULL,
    deployment_name character varying(64) NOT NULL,
    resource_name character varying(64) NOT NULL,
    endpoint_key character varying(128) NOT NULL,
    active boolean NOT NULL,
    model_name character varying(64) NOT NULL
);


ALTER TABLE aoai.owner_catalog OWNER TO admin;

--
-- Name: owner_event_map; Type: TABLE; Schema: aoai; Owner: admin
--

CREATE TABLE aoai.owner_event_map (
    owner_id uuid NOT NULL,
    event_id character varying(50) NOT NULL,
    creator boolean NOT NULL
);


ALTER TABLE aoai.owner_event_map OWNER TO admin;

--
-- Name: event; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.event (
    eventid character varying(50) DEFAULT gen_random_uuid() NOT NULL,
    ownerid uuid DEFAULT gen_random_uuid() NOT NULL,
    eventcode character varying(64) NOT NULL,
    eventmarkdown character varying(4000) NOT NULL,
    startutc timestamp(6) without time zone NOT NULL,
    endutc timestamp(6) without time zone NOT NULL,
    organizername character varying(128) NOT NULL,
    organizeremail character varying(128) NOT NULL,
    eventurl character varying(256) NOT NULL,
    eventurltext character varying(256) NOT NULL,
    maxtokencap integer NOT NULL,
    singlecode boolean NOT NULL,
    dailyrequestcap integer NOT NULL,
    active boolean NOT NULL
);


ALTER TABLE public.event OWNER TO admin;

--
-- Name: event_attendee; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.event_attendee (
    user_id character varying(128) NOT NULL,
    event_id character varying(50) NOT NULL,
    active boolean NOT NULL,
    total_requests integer NOT NULL,
    apikey uuid NOT NULL,
    total_tokens integer
);


ALTER TABLE public.event_attendee OWNER TO admin;

--
-- Name: event_catalog_map; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.event_catalog_map (
    eventid character varying(50) NOT NULL,
    catalogid uuid NOT NULL
);


ALTER TABLE public.event_catalog_map OWNER TO admin;

--
-- Name: owner; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.owner (
    entraid character varying(128) NOT NULL,
    ownerid uuid DEFAULT gen_random_uuid() NOT NULL
);


ALTER TABLE public.owner OWNER TO admin;

--
-- Name: owner_catalog; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.owner_catalog (
    ownerid uuid NOT NULL,
    catalogid uuid DEFAULT gen_random_uuid() NOT NULL,
    friendlyname character varying(64) NOT NULL,
    deploymentname character varying(64) NOT NULL,
    resourcename character varying(64) NOT NULL,
    endpointkey character varying(128) NOT NULL,
    modelclass character varying(64) NOT NULL,
    active boolean NOT NULL
);


ALTER TABLE public.owner_catalog OWNER TO admin;

--
-- Name: owner_event_map; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.owner_event_map (
    ownerid uuid NOT NULL,
    eventid character varying(50) NOT NULL,
    creator boolean NOT NULL
);


ALTER TABLE public.owner_event_map OWNER TO admin;

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
-- Name: event event_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.event
    ADD CONSTRAINT event_pkey PRIMARY KEY (eventid);


--
-- Name: event_attendee eventattendee_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.event_attendee
    ADD CONSTRAINT eventattendee_pkey PRIMARY KEY (user_id, event_id);


--
-- Name: event_catalog_map eventcatalogmap_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.event_catalog_map
    ADD CONSTRAINT eventcatalogmap_pkey PRIMARY KEY (eventid, catalogid);


--
-- Name: owner owner_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner
    ADD CONSTRAINT owner_pkey PRIMARY KEY (ownerid);


--
-- Name: owner_catalog ownercatalog_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner_catalog
    ADD CONSTRAINT ownercatalog_pkey PRIMARY KEY (catalogid);


--
-- Name: owner_event_map ownereventmap_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner_event_map
    ADD CONSTRAINT ownereventmap_pkey PRIMARY KEY (ownerid, eventid);


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
-- Name: event_attendee fk_eventattendee_event; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.event_attendee
    ADD CONSTRAINT fk_eventattendee_event FOREIGN KEY (event_id) REFERENCES public.event(eventid) ON DELETE CASCADE;


--
-- Name: event_catalog_map fk_eventcatalogmap_event; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.event_catalog_map
    ADD CONSTRAINT fk_eventcatalogmap_event FOREIGN KEY (eventid) REFERENCES public.event(eventid) ON DELETE CASCADE;


--
-- Name: event_catalog_map fk_eventcatalogmap_ownercatalog; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.event_catalog_map
    ADD CONSTRAINT fk_eventcatalogmap_ownercatalog FOREIGN KEY (catalogid) REFERENCES public.owner_catalog(catalogid) ON DELETE CASCADE;


--
-- Name: owner_catalog fk_groupmodels_group; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner_catalog
    ADD CONSTRAINT fk_groupmodels_group FOREIGN KEY (ownerid) REFERENCES public.owner(ownerid) ON DELETE CASCADE;


--
-- Name: owner_event_map fk_ownereventmap_event; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner_event_map
    ADD CONSTRAINT fk_ownereventmap_event FOREIGN KEY (eventid) REFERENCES public.event(eventid) ON DELETE CASCADE;


--
-- Name: owner_event_map fk_ownereventmap_owner; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.owner_event_map
    ADD CONSTRAINT fk_ownereventmap_owner FOREIGN KEY (ownerid) REFERENCES public.owner(ownerid) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--
