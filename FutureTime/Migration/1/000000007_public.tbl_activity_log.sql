-- Table: public.tbl_activity_log

--DROP TABLE public.tbl_activity_log;

CREATE TABLE public.tbl_activity_log
(
    uid uuid DEFAULT public.fn_new_uuid() not null,
    id serial,

    activity_detail text COLLATE pg_catalog."default" NOT NULL,
    activity_user character varying(50) COLLATE pg_catalog."default" NOT NULL,
    request_ip  character varying(50) COLLATE pg_catalog."default" NOT NULL,
    remarks text COLLATE pg_catalog."default" NOT NULL,
    reference_json jsonb null,
    activity_date timestamp without time zone NOT NULL ,
    
    CONSTRAINT tbl_activity_log_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.tbl_activity_log
    OWNER to postgres;
