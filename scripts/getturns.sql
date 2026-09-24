-- FUNCTION: public.getturns(date, uuid)

DROP FUNCTION IF EXISTS public.getturns(date, uuid);

CREATE OR REPLACE FUNCTION public.getturns(
	p0 date,
	p1 uuid DEFAULT NULL::uuid)
    RETURNS TABLE(id uuid, name text, dni bigint, medicid uuid, medicname text, "Date" text, "Time" text, timeid uuid, socialwork text, reason text, accessed boolean, ismedic boolean)
    LANGUAGE 'sql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
SELECT t."Id", t."Name", t."Dni", t."MedicId", m."Name" as MedicName, TO_CHAR(t."DateTurn", 'DD/MM/YYYY'), 
			tt."Time", tt."Id" AS TimeId, t."SocialWork", t."Reason", t."Accessed", false
		FROM "Turns" t 
		INNER JOIN "Medics" m ON t."MedicId" = m."Id" 
		INNER JOIN "TimeTurns" tt ON t."TimeId" = tt."Id"
    WHERE (p0 IS NULL OR t."DateTurn"::DATE = p0)
        AND (p1 IS NULL OR m."Id" = p1);
$BODY$;

ALTER FUNCTION public.getturns(date, uuid)
    OWNER TO postgres;

