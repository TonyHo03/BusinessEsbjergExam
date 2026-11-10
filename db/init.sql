CREATE TABLE employee (
                          id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                          name       text NOT NULL,
                          email      text NOT NULL UNIQUE,
                          start_date date NOT NULL,
                          end_date   date,
                          created_at timestamptz NOT NULL DEFAULT now()
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
                               mon numeric(4,2) NOT NULL,
                               tue numeric(4,2) NOT NULL,
                               wed numeric(4,2) NOT NULL,
                               thu numeric(4,2) NOT NULL,
                               fri numeric(4,2) NOT NULL,
                               sat numeric(4,2) NOT NULL DEFAULT 0,
                               sun numeric(4,2) NOT NULL DEFAULT 0,
                               UNIQUE (employee_id, effective_from)
);

CREATE TABLE day_off (
                         id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                         date       date NOT NULL UNIQUE,
                         name       text NOT NULL,
                         is_company boolean NOT NULL DEFAULT false
);

-- Regelparametre for Business Esbjerg (KL 05.11-modellen)
INSERT INTO rule_parameter (key, value, effective_from) VALUES
                                                            ('weeks1to5.days_per_month',      '2.08',  '2020-09-01'),
                                                            ('week6.hours_per_month',         '3.083', '2020-01-01'),
                                                            ('main_holiday.days',             '15',    '2020-09-01'),
                                                            ('notice.main_holiday_months',    '3',     '2020-09-01'),
                                                            ('notice.other_months',           '1',     '2020-09-01'),
                                                            ('payout.weeks1to5_percent',      '12.5',  '2020-09-01'),
                                                            ('payout.week6_percent',          '2.5',   '2020-01-01'),
                                                            ('supplement.base_percent',       '1',     '2020-09-01'),
                                                            ('supplement.raised_percent',     '0.95',  '2020-09-01'),
                                                            ('supplement.raised_percent',     '1.48',  '2024-05-01');

-- Helligdage og firmafridage 2026–2027
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

-- Testmedarbejder med Business Esbjergs standardskema
INSERT INTO employee (id, name, email, start_date) VALUES
    ('11111111-1111-1111-1111-111111111111', 'Mette Jensen', 'mette@businessesbjerg.dk', '2024-01-01');

INSERT INTO work_schedule (employee_id, effective_from, mon, tue, wed, thu, fri) VALUES
    ('11111111-1111-1111-1111-111111111111', '2024-01-01', 7.5, 7.5, 7.5, 7.5, 7);