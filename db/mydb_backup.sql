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
-- Name: fn_addapprovedpayment(integer, integer, numeric, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_addapprovedpayment(p_paymentrequestid integer, p_customerid integer, p_amount numeric, p_paymenttypesid integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO public.approvedpayments (paymentrequestid, amount, customerid)
    VALUES (p_PaymentRequestId, p_Amount, p_CustomerId);
END;
$$;


ALTER FUNCTION public.fn_addapprovedpayment(p_paymentrequestid integer, p_customerid integer, p_amount numeric, p_paymenttypesid integer) OWNER TO postgres;

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


ALTER FUNCTION public.fn_addpaymentrequest(customer_id integer, amount numeric, payment_type_id integer, is_confirmed boolean, status_id integer) OWNER TO postgres;

--
-- Name: fn_confirmpayment(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_confirmpayment(payment_request_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE PaymentRequests
    SET IsConfirmed = true
    WHERE PaymentRequestId = payment_request_id;
END;
$$;


ALTER FUNCTION public.fn_confirmpayment(payment_request_id integer) OWNER TO postgres;

--
-- Name: fn_getauthorizedpayments(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_getauthorizedpayments() RETURNS TABLE(paymentid integer, paymentapprovaldate timestamp without time zone, paymentamount numeric, paymentcustomerid integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY SELECT 
        PaymentRequestId::integer, 
        ApprovalDate::timestamp without time zone, 
        Amount::numeric, 
        CustomerId::integer
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


ALTER FUNCTION public.fn_getpaymentrequestbyid(id integer) OWNER TO postgres;

--
-- Name: fn_getpaymentstatusbyid(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_getpaymentstatusbyid(payment_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    status_id INT;
BEGIN
    SELECT StatusId INTO status_id
    FROM PaymentRequests
    WHERE PaymentRequestId = payment_id;
    
    RETURN status_id;
END;
$$;


ALTER FUNCTION public.fn_getpaymentstatusbyid(payment_id integer) OWNER TO postgres;

--
-- Name: fn_isconfirmed(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_isconfirmed(payment_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
DECLARE
    is_confirmed BOOLEAN;
BEGIN
    SELECT IsConfirmed INTO is_confirmed
    FROM PaymentRequests
    WHERE PaymentRequestId = payment_id;
    
    RETURN is_confirmed;
END;
$$;


ALTER FUNCTION public.fn_isconfirmed(payment_id integer) OWNER TO postgres;

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

--
-- Name: fn_updatepaymentstatus(integer, integer, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.fn_updatepaymentstatus(payment_request_id integer, status_id integer, _details character varying) RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE PaymentRequests 
    SET StatusId = status_id,
	details = _details
    WHERE PaymentRequestId = payment_request_id;
END;
$$;


ALTER FUNCTION public.fn_updatepaymentstatus(payment_request_id integer, status_id integer, _details character varying) OWNER TO postgres;

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
    isconfirmed boolean DEFAULT false,
    details character varying(200)
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
141	2024-04-19 09:31:05.41031-03	10.00	2
142	2024-04-19 09:43:03.602709-03	100.00	2
144	2024-04-19 09:43:06.233463-03	10000.00	2
145	2024-04-19 09:43:52.993106-03	10.50	2
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

COPY public.paymentrequests (paymentrequestid, customerid, amount, requestdate, statusid, paymenttypesid, isconfirmed, details) FROM stdin;
90	2	10.00	2024-04-17 15:48:54.710044-03	2	0	f	\N
91	2	10.00	2024-04-17 15:50:29.741251-03	2	0	f	\N
92	2	10.00	2024-04-17 15:54:55.233852-03	3	0	f	\N
125	2	10.00	2024-04-18 08:21:13.10957-03	1	0	t	\N
93	2	10.00	2024-04-17 15:56:13.164088-03	3	0	t	\N
126	2	10.00	2024-04-18 08:32:00.464926-03	3	0	f	\N
94	2	10.00	2024-04-17 15:58:02.759585-03	3	0	t	\N
127	2	10.00	2024-04-18 08:34:35.535301-03	3	0	f	\N
95	2	10.00	2024-04-17 15:59:41.039297-03	3	0	t	\N
96	2	10.00	2024-04-17 16:00:57.07669-03	3	0	t	\N
97	2	10.00	2024-04-17 16:01:25.238918-03	3	0	t	\N
137	2	10.00	2024-04-19 09:21:01.469652-03	2	0	f	There was an error trying to approve the payment.
98	2	10.00	2024-04-17 16:06:00.720537-03	3	0	t	\N
128	2	10.00	2024-04-18 08:40:47.720903-03	1	0	t	\N
99	2	10.00	2024-04-17 16:06:14.975007-03	3	0	t	\N
100	2	10.00	2024-04-17 16:06:41.409798-03	3	0	t	\N
101	2	10.00	2024-04-17 16:07:12.85808-03	3	0	t	\N
144	2	10000.00	2024-04-19 09:42:56.212563-03	1	0	t	Payment approved
102	2	10.00	2024-04-17 16:11:58.250275-03	3	0	t	\N
138	2	10.00	2024-04-19 09:22:13.495349-03	3	0	f	Pending
103	2	10.00	2024-04-17 16:14:03.599495-03	3	0	t	\N
129	2	10.00	2024-04-19 08:55:19.842013-03	1	0	t	\N
104	2	10.00	2024-04-17 16:17:05.997251-03	3	0	t	\N
43	1	10.00	2024-04-17 09:52:44.923047-03	3	0	f	\N
44	1	10.00	2024-04-17 09:53:42.612532-03	3	0	f	\N
45	1	10.00	2024-04-17 09:54:14.145499-03	3	0	f	\N
46	1	10.00	2024-04-17 09:56:22.363277-03	3	0	f	\N
47	1	10.00	2024-04-17 09:57:15.719694-03	3	0	f	\N
48	2	10.00	2024-04-17 09:58:09.201388-03	3	0	f	\N
49	2	10.00	2024-04-17 10:41:53.369986-03	3	0	f	\N
50	2	10.00	2024-04-17 10:42:23.104423-03	3	0	f	\N
51	2	10.00	2024-04-17 10:43:02.875261-03	3	0	f	\N
52	2	10.00	2024-04-17 10:44:45.143923-03	3	0	f	\N
53	2	10.00	2024-04-17 10:45:23.741574-03	4	0	f	\N
54	2	10.00	2024-04-17 10:54:21.8793-03	3	0	f	\N
55	2	10.00	2024-04-17 10:54:54.790563-03	3	0	f	\N
56	2	10.00	2024-04-17 10:55:28.161137-03	3	0	f	\N
57	2	10.00	2024-04-17 11:04:37.740249-03	3	0	f	\N
58	2	10.00	2024-04-17 11:06:01.993552-03	3	0	f	\N
59	2	10.00	2024-04-17 11:09:16.081495-03	3	0	f	\N
60	2	10.00	2024-04-17 11:10:28.657652-03	3	0	f	\N
61	2	10.00	2024-04-17 11:12:17.925122-03	3	0	f	\N
62	2	10.00	2024-04-17 11:13:04.206854-03	3	0	f	\N
63	2	10.00	2024-04-17 11:14:07.513044-03	3	0	f	\N
64	2	10.00	2024-04-17 11:16:06.386567-03	3	0	f	\N
65	2	10.00	2024-04-17 11:20:20.553542-03	3	0	f	\N
66	2	10.00	2024-04-17 11:28:37.407975-03	3	0	f	\N
67	2	10.00	2024-04-17 11:28:59.471087-03	3	0	f	\N
68	2	10.00	2024-04-17 11:29:55.140507-03	3	0	f	\N
69	2	10.00	2024-04-17 11:30:46.681747-03	3	0	f	\N
70	2	10.00	2024-04-17 11:40:39.485055-03	3	0	f	\N
71	2	10.00	2024-04-17 11:40:47.362439-03	3	0	f	\N
72	2	10.00	2024-04-17 11:40:54.014704-03	3	0	f	\N
73	2	10.00	2024-04-17 11:40:54.706685-03	3	0	f	\N
74	2	10.00	2024-04-17 11:40:55.295086-03	3	0	f	\N
75	2	10.00	2024-04-17 11:40:55.844272-03	3	0	f	\N
76	2	10.00	2024-04-17 11:41:55.141503-03	3	0	f	\N
77	2	10.00	2024-04-17 11:42:37.388762-03	3	0	f	\N
78	2	10.00	2024-04-17 11:43:15.06084-03	3	0	f	\N
79	2	10.00	2024-04-17 11:43:40.949592-03	3	0	f	\N
80	2	10.00	2024-04-17 11:44:27.41006-03	3	0	f	\N
81	2	10.00	2024-04-17 11:49:57.418133-03	3	0	f	\N
82	2	10.00	2024-04-17 11:55:15.358298-03	3	0	f	\N
83	2	10.00	2024-04-17 11:55:59.743686-03	3	0	f	\N
84	2	10.00	2024-04-17 11:56:54.87934-03	3	0	t	\N
85	2	10.00	2024-04-17 12:01:28.559491-03	3	0	f	\N
86	2	10.00	2024-04-17 12:03:48.450985-03	3	0	f	\N
87	2	10.00	2024-04-17 12:05:57.054646-03	3	0	t	\N
105	2	10.00	2024-04-17 16:19:45.785818-03	3	0	t	\N
88	2	10.00	2024-04-17 12:12:40.573562-03	1	0	t	\N
106	2	10.00	2024-04-17 16:19:57.332794-03	3	0	t	\N
89	2	10.00	2024-04-17 12:13:28.383066-03	1	0	t	\N
130	2	10.00	2024-04-19 09:04:08.736803-03	2	0	f	\N
107	2	10.00	2024-04-17 16:21:30.015562-03	3	0	t	\N
131	2	10.00	2024-04-19 09:13:15.983297-03	3	0	f	\N
108	2	10.00	2024-04-17 16:22:35.481602-03	3	0	t	\N
132	2	10.00	2024-04-19 09:13:48.618484-03	3	0	f	\N
109	2	10.00	2024-04-17 16:25:52.843232-03	3	0	t	\N
133	2	10.00	2024-04-19 09:15:04.13512-03	3	0	f	\N
110	2	10.00	2024-04-17 16:28:24.337767-03	3	0	t	\N
111	2	10.00	2024-04-17 16:29:30.027917-03	3	0	f	\N
112	2	10.00	2024-04-17 16:33:10.797235-03	3	0	f	\N
113	2	10.00	2024-04-17 16:33:56.386457-03	3	0	f	\N
114	2	10.00	2024-04-17 16:35:37.416386-03	3	0	f	\N
115	2	10.00	2024-04-17 16:37:15.939996-03	3	0	f	\N
116	2	10.00	2024-04-17 16:38:13.598609-03	3	0	f	\N
117	2	10.00	2024-04-17 16:39:22.055251-03	3	0	f	\N
118	2	10.00	2024-04-17 16:40:37.426946-03	3	0	f	\N
119	2	10.00	2024-04-17 16:41:38.969473-03	3	0	f	\N
120	2	10.00	2024-04-17 16:44:15.241489-03	3	0	f	\N
121	2	10.00	2024-04-17 16:46:09.753155-03	3	0	f	\N
122	2	10.00	2024-04-17 16:48:01.858422-03	3	0	f	\N
123	2	10.00	2024-04-17 16:50:53.804658-03	3	0	f	\N
124	2	10.00	2024-04-18 08:19:13.661148-03	3	0	f	\N
134	2	10.00	2024-04-19 09:17:11.832484-03	3	0	f	\N
142	2	100.00	2024-04-19 09:42:53.357415-03	1	0	t	Payment approved
141	2	10.00	2024-04-19 09:30:55.17377-03	1	0	t	Payment approved
139	2	10.00	2024-04-19 09:26:36.882332-03	2	0	f	There was an error trying to approve the payment.
140	2	10.00	2024-04-19 09:29:06.667928-03	1	0	t	Payment approved
135	2	10.00	2024-04-19 09:17:59.632004-03	1	0	t	Payment approved
136	2	10.00	2024-04-19 09:20:20.438097-03	2	0	f	There was an error trying to approve the payment. Status code: Unauthorized, Content: System.Net.Http.HttpConnectionResponseContent
143	2	1000.00	2024-04-19 09:42:54.76705-03	3	0	t	Calling the processor.
145	2	10.50	2024-04-19 09:43:42.758045-03	4	0	f	Payment approved
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

SELECT pg_catalog.setval('public.paymentrequests_paymentrequestid_seq', 145, true);


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

