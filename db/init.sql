-- ===== Tabeller =====

CREATE TABLE employee (
                          id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                          name       text NOT NULL,
                          email      text NOT NULL UNIQUE,
                          password_hash text NOT NULL,
                          role       text NOT NULL,
                          start_date date NOT NULL,
                          end_date   date,
                          created_at timestamptz NOT NULL DEFAULT now(),
                          status     text NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Inactive'))
);


CREATE TABLE rule_parameter (
                                id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                key            text NOT NULL,
                                value          text NOT NULL,
                                effective_from date NOT NULL,
                                UNIQUE (key, effective_from)
);

CREATE TABLE work_schedule (
                               id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                               employee_id    uuid NOT NULL REFERENCES employee(id),
                               effective_from date NOT NULL,
                               effective_to   date,                  -- NULL = indtil videre
                               mon_start time, mon_end time,         -- NULL = ikke en arbejdsdag
                               tue_start time, tue_end time,
                               wed_start time, wed_end time,
                               thu_start time, thu_end time,
                               fri_start time, fri_end time,
                               UNIQUE (employee_id, effective_from),
                               CHECK (effective_to IS NULL OR effective_to >= effective_from),
                               CHECK ((mon_start IS NULL) = (mon_end IS NULL) AND (mon_end IS NULL OR mon_end > mon_start)),
                               CHECK ((tue_start IS NULL) = (tue_end IS NULL) AND (tue_end IS NULL OR tue_end > tue_start)),
                               CHECK ((wed_start IS NULL) = (wed_end IS NULL) AND (wed_end IS NULL OR wed_end > wed_start)),
                               CHECK ((thu_start IS NULL) = (thu_end IS NULL) AND (thu_end IS NULL OR thu_end > thu_start)),
                               CHECK ((fri_start IS NULL) = (fri_end IS NULL) AND (fri_end IS NULL OR fri_end > fri_start))
);

CREATE TABLE day_off (
                         id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                         date       date NOT NULL UNIQUE,
                         name       text NOT NULL,
                         is_company boolean NOT NULL DEFAULT false
);

CREATE TABLE holiday_request (
                                 id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                 employee_id      uuid NOT NULL REFERENCES employee(id),
                                 account          text NOT NULL CHECK (account IN ('Weeks1to5', 'Week6')),
                                 start_date       date NOT NULL,
                                 end_date         date NOT NULL,
                                 working_days     integer NOT NULL,
                                 hours            numeric(6,2) NOT NULL,
                                 status           text NOT NULL CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Cancelled')),
                                 comment          text,
                                 rejection_reason text,
                                 decided_by       uuid REFERENCES employee(id),
                                 decided_at       timestamptz,
                                 created_at       timestamptz NOT NULL DEFAULT now(),
                                 CHECK (end_date >= start_date)
);

CREATE TABLE transfer_request (
                                  id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                  employee_id       uuid NOT NULL REFERENCES employee(id),
                                  from_earning_year int  NOT NULL,          -- uge 1–5-perioden dagene flyttes fra
                                  days              numeric(6,2) NOT NULL CHECK (days > 0),
                                  status            text NOT NULL CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Cancelled')),
                                  comment           text,
                                  rejection_reason  text,
                                  decided_by        uuid REFERENCES employee(id),
                                  decided_at        timestamptz,
                                  created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE ledger_entry (
                              id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                              employee_id  uuid NOT NULL REFERENCES employee(id),
                              account      text NOT NULL CHECK (account IN ('Weeks1to5', 'Week6')),
                              earning_year int  NOT NULL,
                              type         text NOT NULL CHECK (type IN ('Usage', 'Payout', 'Expiry', 'Adjustment', 'Transfer')),
                              days         numeric(8,4) NOT NULL,  -- saldoens enhed: ferie er en ret til dage
                              hours        numeric(7,3) NOT NULL,  -- de timer det svarede til på datoen
                              entry_date   date NOT NULL,
                              request_id   uuid REFERENCES holiday_request(id),
                              transfer_id  uuid REFERENCES transfer_request(id)
);

-- ===== Regelparametre for Business Esbjerg (KL 05.11-modellen) =====

INSERT INTO rule_parameter (key, value, effective_from) VALUES
                                                            ('weeks1to5.days_per_month',   '2.08',  '2020-09-01'),
                                                            ('week6.hours_per_month',      '3.083', '2020-01-01'),
                                                            ('main_holiday.days',          '15',    '2020-09-01'),
                                                            ('notice.main_holiday_months', '3',     '2020-09-01'),
                                                            ('notice.other_months',        '1',     '2020-09-01'),
                                                            ('transfer.enabled',           'true',  '2020-09-01'),
                                                            ('transfer.max_days',          '5',     '2020-09-01');

-- ===== Helligdage og firmafridage 2026–2027 =====

INSERT INTO day_off (date, name, is_company) VALUES
                                                 ('2026-01-01', 'Nytårsdag', false),
                                                 ('2026-04-02', 'Skærtorsdag', false),
                                                 ('2026-04-03', 'Langfredag', false),
                                                 ('2026-04-05', 'Påskedag', false),
                                                 ('2026-04-06', '2. påskedag', false),
                                                 ('2026-05-14', 'Kristi himmelfartsdag', false),
                                                 ('2026-05-24', 'Pinsedag', false),
                                                 ('2026-05-25', '2. pinsedag', false),
                                                 ('2026-12-24', 'Juleaftensdag', true),
                                                 ('2026-12-25', 'Juledag', false),
                                                 ('2026-12-26', '2. juledag', false),
                                                 ('2026-12-31', 'Nytårsaftensdag', true),
                                                 ('2027-01-01', 'Nytårsdag', false),
                                                 ('2027-03-25', 'Skærtorsdag', false),
                                                 ('2027-03-26', 'Langfredag', false),
                                                 ('2027-03-28', 'Påskedag', false),
                                                 ('2027-03-29', '2. påskedag', false),
                                                 ('2027-05-06', 'Kristi himmelfartsdag', false),
                                                 ('2027-05-16', 'Pinsedag', false),
                                                 ('2027-05-17', '2. pinsedag', false),
                                                 ('2027-12-24', 'Juleaftensdag', true),
                                                 ('2027-12-25', 'Juledag', false),
                                                 ('2027-12-26', '2. juledag', false),
                                                 ('2027-12-31', 'Nytårsaftensdag', true);

-- ===== Testdata: Mette fra skitsen =====

INSERT INTO employee (id, name, email, password_hash, role, start_date) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Mette Jensen', 'mette@businessesbjerg.dk', '$argon2id$v=19$m=65536,t=3,p=1$d8IU1aKhQ4sfhsYCQrMv5A$lA+WVRNG8oJjfb9GB3jZXsNXkDR7nIKz/E49TJcDdZk', 'Admin', '2025-09-01');

INSERT INTO work_schedule (employee_id, effective_from, mon_start, mon_end, tue_start, tue_end, wed_start, wed_end, thu_start, thu_end, fri_start, fri_end) VALUES
    ('11111111-1111-1111-1111-111111111111', '2025-09-01',
     '08:00', '15:30', '08:00', '15:30', '08:00', '15:30', '08:00', '15:30', '08:00', '15:00');

-- Godkendt sommerferie 6.7–24.7.2026: 15 dage, 111 t fra optjeningsår 2025
INSERT INTO holiday_request (id, employee_id, account, start_date, end_date, working_days, hours, status, comment) VALUES
    ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111',
     'Weeks1to5', '2026-07-06', '2026-07-24', 15, 111, 'Approved', 'Sommerferie');

INSERT INTO ledger_entry (employee_id, account, earning_year, type, days, hours, entry_date, request_id) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Weeks1to5', 2025, 'Usage', -15, -111, '2026-07-06',
     '22222222-2222-2222-2222-222222222222');