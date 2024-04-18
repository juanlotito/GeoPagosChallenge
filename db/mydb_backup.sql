--
-- PostgreSQL database dump
--

-- Dumped from database version 15.4
-- Dumped by pg_dump version 15.4

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
SET search_path = public;

--
-- Name: clientinfo; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.clientinfo AS (
	customerid integer,
	customertypeid integer,
	customertypedescription text,
	name text,
	email text
);


ALTER TYPE public.clientinfo OWNER TO postgres;

--
-- Name: fn_addpaymentrequest(integer, numeric, integer, boolean, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_addpaymentrequest(customer_id integer, amount numeric, payment_type_id integer, is_confirmed boolean, status_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    new_payment_request_id INT;
BEGIN
    INSERT INTO PaymentRequests (CustomerId, Amount, PaymentTypesId, IsConfirmed, StatusId, RequestDate)
    VALUES (customer_id, amount, payment_type_id, is_confirmed, status_id, NOW())
    RETURNING PaymentRequestId INTO new_payment_request_id;
    
    RETURN new_payment_request_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_AddApprovedPayment(
    p_PaymentRequestId INTEGER,
    p_CustomerId INTEGER,
    p_Amount NUMERIC,
    p_PaymentTypesId INTEGER
)
RETURNS VOID AS
$$
BEGIN
    INSERT INTO public.approvedpayments (paymentrequestid, amount, customerid)
    VALUES (p_PaymentRequestId, p_Amount, p_CustomerId);
END;
$$
LANGUAGE plpgsql;

ALTER FUNCTION  public.fn_AddApprovedPayment OWNER TO postgres;


ALTER FUNCTION public.fn_addpaymentrequest(customer_id integer, amount numeric, payment_type_id integer, is_confirmed boolean, status_id integer) OWNER TO postgres;

--
-- Name: fn_getauthorizedpayments(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_updatepaymentstatus(
    payment_request_id integer,
    status_id integer)
RETURNS void
LANGUAGE plpgsql  
COST 100
VOLATILE PARALLEL UNSAFE
AS $BODY$
BEGIN
    UPDATE PaymentRequests
    SET StatusId = status_id
    WHERE PaymentRequestId = payment_request_id;
END;
$BODY$;

ALTER FUNCTION public.fn_updatepaymentstatus(integer, integer)
    OWNER TO postgres;

CREATE FUNCTION public.fn_getauthorizedpayments() RETURNS TABLE(paymentrequestid integer, approvaldate timestamp without time zone, amount numeric, customerid integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY SELECT PaymentRequestId, ApprovalDate, Amount, CustomerId
    FROM ApprovedPayments;
END;
$$;


ALTER FUNCTION public.fn_getauthorizedpayments() OWNER TO postgres;

--
-- Name: fn_getclientbyid(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_getclientbyid(customer_id integer) RETURNS TABLE(customerid integer, customertypeid integer, customertypedescription character varying, name character varying, email character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY SELECT 
        cu.CustomerId,
        cu.CustomerTypeId,
        ct.CustomerTypeDescription,
        cu.Name,
        cu.Email
    FROM Customers cu
    LEFT JOIN CustomerTypes ct ON cu.CustomerTypeId = ct.CustomerTypeId
    WHERE cu.CustomerId = customer_id;
END;
$$;


ALTER FUNCTION public.fn_getclientbyid(customer_id integer) OWNER TO postgres;

--
-- Name: fn_getpaymentrequestbyid(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_getpaymentrequestbyid(id integer) RETURNS TABLE(paymentrequestid integer, statusid character varying, isconfirmed boolean, amount numeric, customerid integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY SELECT p.PaymentRequestId, p.StatusId, p.IsConfirmed, p.Amount, p.CustomerId
    FROM PaymentRequests p
    WHERE p.PaymentRequestId = id;
END;
$$;

ALTER FUNCTION public.fn_getpaymentrequestbyid(payment_request_id integer) OWNER TO postgres;

--
-- Name: fn_reversepayment(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_reversepayment(payment_request_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE PaymentRequests
    SET StatusId = 4,
        IsConfirmed = FALSE
    WHERE PaymentRequestId = payment_request_id;
END;
$$;


ALTER FUNCTION public.fn_reversepayment(payment_request_id integer) OWNER TO postgres;

--
-- Name: fn_updatepaymentrequest(integer, integer, boolean); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_updatepaymentrequest(payment_request_id integer, new_status_id integer, is_confirmed boolean) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE PaymentRequests
    SET StatusId = new_status_id,
        IsConfirmed = is_confirmed
    WHERE PaymentRequestId = payment_request_id;
END;
$$;


ALTER FUNCTION public.fn_updatepaymentrequest(payment_request_id integer, new_status_id integer, is_confirmed boolean) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: approvedpayments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.approvedpayments (
    paymentrequestid integer NOT NULL,
    approvaldate timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    amount numeric(10,2) NOT NULL,
    customerid integer NOT NULL
);


ALTER TABLE public.approvedpayments OWNER TO postgres;

--
-- Name: customers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.customers (
    customerid integer NOT NULL,
    customertypeid integer NOT NULL,
    name character varying(255) NOT NULL,
    email character varying(255) NOT NULL
);


ALTER TABLE public.customers OWNER TO postgres;

--
-- Name: customers_customerid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.customers_customerid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.customers_customerid_seq OWNER TO postgres;

--
-- Name: customers_customerid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.customers_customerid_seq OWNED BY public.customers.customerid;


--
-- Name: customertypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.customertypes (
    customertypeid integer NOT NULL,
    customertypedescription character varying(200)
);


ALTER TABLE public.customertypes OWNER TO postgres;

--
-- Name: customertypes_customertypeid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.customertypes_customertypeid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.customertypes_customertypeid_seq OWNER TO postgres;

--
-- Name: customertypes_customertypeid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.customertypes_customertypeid_seq OWNED BY public.customertypes.customertypeid;


--
-- Name: paymentrequests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.paymentrequests (
    paymentrequestid integer NOT NULL,
    customerid integer NOT NULL,
    amount numeric(10,2) NOT NULL,
    requestdate timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    statusid character varying(50),
    paymenttypesid character varying(50),
    isconfirmed boolean DEFAULT false
);


ALTER TABLE public.paymentrequests OWNER TO postgres;

--
-- Name: paymentrequests_paymentrequestid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.paymentrequests_paymentrequestid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.paymentrequests_paymentrequestid_seq OWNER TO postgres;

--
-- Name: paymentrequests_paymentrequestid_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.paymentrequests_paymentrequestid_seq OWNED BY public.paymentrequests.paymentrequestid;


--
-- Name: paymenttypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.paymenttypes (
    paymenttypeid integer NOT NULL,
    paymenttypedescription character varying(100)
);


ALTER TABLE public.paymenttypes OWNER TO postgres;

--
-- Name: statustypes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.statustypes (
    statustypeid integer NOT NULL,
    statustypedescription character varying(100)
);


ALTER TABLE public.statustypes OWNER TO postgres;

--
-- Name: customers customerid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers ALTER COLUMN customerid SET DEFAULT nextval('public.customers_customerid_seq'::regclass);


--
-- Name: customertypes customertypeid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customertypes ALTER COLUMN customertypeid SET DEFAULT nextval('public.customertypes_customertypeid_seq'::regclass);


--
-- Name: paymentrequests paymentrequestid; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paymentrequests ALTER COLUMN paymentrequestid SET DEFAULT nextval('public.paymentrequests_paymentrequestid_seq'::regclass);


--
-- Data for Name: approvedpayments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.approvedpayments (paymentrequestid, approvaldate, amount, customerid) FROM stdin;
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.customers (customerid, customertypeid, name, email) FROM stdin;
1	1	Jhon Doe	jdoe@example.com
2	2	Charles Doe	cdoe@example.com
\.


--
-- Data for Name: customertypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.customertypes (customertypeid, customertypedescription) FROM stdin;
1	Primero
2	Segundo
\.


--
-- Data for Name: paymentrequests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.paymentrequests (paymentrequestid, customerid, amount, requestdate, statusid, paymenttypesid, isconfirmed) FROM stdin;
\.


--
-- Data for Name: paymenttypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.paymenttypes (paymenttypeid, paymenttypedescription) FROM stdin;
\.


--
-- Data for Name: statustypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.statustypes (statustypeid, statustypedescription) FROM stdin;
1	Approved
2	Denied
3	Pending
4	Reverted
\.


--
-- Name: customers_customerid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.customers_customerid_seq', 1, false);


--
-- Name: customertypes_customertypeid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.customertypes_customertypeid_seq', 1, false);


--
-- Name: paymentrequests_paymentrequestid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.paymentrequests_paymentrequestid_seq', 42, true);


--
-- Name: approvedpayments approvedpayments_paymentrequestid_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.approvedpayments
    ADD CONSTRAINT approvedpayments_paymentrequestid_key UNIQUE (paymentrequestid);


--
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (customerid);


--
-- Name: customertypes customertypes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customertypes
    ADD CONSTRAINT customertypes_pkey PRIMARY KEY (customertypeid);


--
-- Name: paymentrequests paymentrequests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paymentrequests
    ADD CONSTRAINT paymentrequests_pkey PRIMARY KEY (paymentrequestid);


--
-- Name: paymenttypes paymenttypes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paymenttypes
    ADD CONSTRAINT paymenttypes_pkey PRIMARY KEY (paymenttypeid);


--
-- Name: statustypes statustypes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.statustypes
    ADD CONSTRAINT statustypes_pkey PRIMARY KEY (statustypeid);


--
-- Name: approvedpayments approvedpayments_customerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.approvedpayments
    ADD CONSTRAINT approvedpayments_customerid_fkey FOREIGN KEY (customerid) REFERENCES public.customers(customerid);


--
-- Name: approvedpayments approvedpayments_paymentrequestid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.approvedpayments
    ADD CONSTRAINT approvedpayments_paymentrequestid_fkey FOREIGN KEY (paymentrequestid) REFERENCES public.paymentrequests(paymentrequestid);


--
-- Name: paymentrequests paymentrequests_customerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.paymentrequests
    ADD CONSTRAINT paymentrequests_customerid_fkey FOREIGN KEY (customerid) REFERENCES public.customers(customerid);


--
-- PostgreSQL database dump complete
--

