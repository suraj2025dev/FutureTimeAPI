create table pms.tbl_property
(
    id serial,
    full_name character varying(500) COLLATE pg_catalog."default",
    url character varying(500) COLLATE pg_catalog."default",
	
    is_active boolean NOT NULL DEFAULT true,
    is_deleted boolean NOT NULL DEFAULT false,
    created_by character varying(50) COLLATE pg_catalog."default" NOT null default 'system',
    created_date timestamp without time zone NOT NULL DEFAULT now(),
    generated_by character varying(50) COLLATE pg_catalog."default" NOT null default 'system',
    generated_on timestamp without time zone NOT NULL DEFAULT now(),
    CONSTRAINT tbl_property_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE pms.tbl_property
    OWNER to postgres;
