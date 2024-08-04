-- Table: public.tbl_error_log

-- DROP TABLE public.tbl_error_log;

CREATE TABLE public.tbl_error_log
(
    uid uuid DEFAULT public.fn_new_uuid() not null,
    id serial,

    guid character varying(100) COLLATE pg_catalog."default" NOT NULL,
    exception jsonb NOT NULL,
    created_date timestamp without time zone NOT NULL DEFAULT now(),
    CONSTRAINT tbl_error_log_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.tbl_error_log
    OWNER to postgres;
