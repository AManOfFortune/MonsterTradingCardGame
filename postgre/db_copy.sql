--
-- PostgreSQL database dump
--

-- Dumped from database version 14.5
-- Dumped by pg_dump version 14.5

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

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: cards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cards (
    card_id uuid NOT NULL,
    name character varying(255),
    damage real,
    pack_id integer,
    owner integer,
    posindeck integer DEFAULT 0 NOT NULL,
    CONSTRAINT cards_posindeck_check CHECK (((posindeck >= 0) AND (posindeck <= 4)))
);


ALTER TABLE public.cards OWNER TO postgres;

--
-- Name: packs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.packs (
    pack_id integer NOT NULL,
    num_of_cards integer NOT NULL,
    bought boolean DEFAULT false
);


ALTER TABLE public.packs OWNER TO postgres;

--
-- Name: packs_pack_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.packs_pack_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.packs_pack_id_seq OWNER TO postgres;

--
-- Name: packs_pack_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.packs_pack_id_seq OWNED BY public.packs.pack_id;


--
-- Name: tradingdeals; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tradingdeals (
    deal_id uuid NOT NULL,
    card_to_trade uuid NOT NULL,
    wanted_type character varying(255) NOT NULL,
    wanted_element character varying(255),
    wanted_min_damage integer
);


ALTER TABLE public.tradingdeals OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    username character varying(100) NOT NULL,
    password character varying(100) NOT NULL,
    role character varying DEFAULT 'player'::character varying NOT NULL,
    coins integer DEFAULT 20,
    bio text,
    image character varying(255),
    wins integer DEFAULT 0 NOT NULL,
    losses integer DEFAULT 0 NOT NULL,
    elo integer DEFAULT 100 NOT NULL,
    CONSTRAINT users_coins_check CHECK ((coins >= 0))
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.users_user_id_seq OWNER TO postgres;

--
-- Name: users_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;


--
-- Name: packs pack_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.packs ALTER COLUMN pack_id SET DEFAULT nextval('public.packs_pack_id_seq'::regclass);


--
-- Name: users user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);


--
-- Data for Name: cards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.cards (card_id, name, damage, pack_id, owner, posindeck) FROM stdin;
\.


--
-- Data for Name: packs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.packs (pack_id, num_of_cards, bought) FROM stdin;
\.


--
-- Data for Name: tradingdeals; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tradingdeals (deal_id, card_to_trade, wanted_type, wanted_element, wanted_min_damage) FROM stdin;
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (user_id, username, password, role, coins, bio, image, wins, losses, elo) FROM stdin;
\.


--
-- Name: packs_pack_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.packs_pack_id_seq', 81, true);


--
-- Name: users_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_user_id_seq', 53, true);


--
-- Name: cards cards_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_pkey PRIMARY KEY (card_id);


--
-- Name: packs packs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.packs
    ADD CONSTRAINT packs_pkey PRIMARY KEY (pack_id);


--
-- Name: tradingdeals tradingdeals_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tradingdeals
    ADD CONSTRAINT tradingdeals_pkey PRIMARY KEY (deal_id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: cards cards_owner_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_owner_fkey FOREIGN KEY (owner) REFERENCES public.users(user_id);


--
-- Name: cards cards_pack_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_pack_id_fkey FOREIGN KEY (pack_id) REFERENCES public.packs(pack_id);


--
-- Name: tradingdeals tradingdeals_card_to_trade_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tradingdeals
    ADD CONSTRAINT tradingdeals_card_to_trade_fkey FOREIGN KEY (card_to_trade) REFERENCES public.cards(card_id);


--
-- PostgreSQL database dump complete
--

