create table public.tbl_property
(
    id serial,
    property_name character varying(500) COLLATE pg_catalog."default" not null,
    property_address character varying(500) COLLATE pg_catalog."default" not null,
    property_type character varying(100) COLLATE pg_catalog."default" not null,
    basic_facilities jsonb,
	google_map_link character varying(500) COLLATE pg_catalog."default" not null,
	images character varying(500)[] COLLATE pg_catalog."default" not null,
	property_overview character varying(2500)[] COLLATE pg_catalog."default" not null,
    house_rules jsonb

    is_active boolean NOT NULL DEFAULT true,
    is_deleted boolean NOT NULL DEFAULT false,
    created_by character varying(50) COLLATE pg_catalog."default" NOT null default 'system',
    created_date timestamp without time zone NOT NULL DEFAULT now(),
    generated_by character varying(50) COLLATE pg_catalog."default" NOT null default 'system',
    generated_on timestamp without time zone NOT NULL DEFAULT now(),
    CONSTRAINT tbl_property_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.tbl_property
    OWNER to postgres;
