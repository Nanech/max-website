-- take extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

create table if not exists categories (
    id SMALLSERIAL PRIMARY key,
    name text not null unique
);

create table if not exists photos (
    id uuid primary key default uuid_generate_v4(),
    s3_file_path text not null unique,
    uploaded_at timestamp with time zone default current_timestamp,
    shoot_year smallint not null
);

create table if not exists photo_categories (
    photo_id uuid references photos(id) on delete cascade,
    category_id smallint references categories(id) on delete cascade,
    primary key (photo_id, category_id)
);

-- default values
do $$
begin 
    if not exists (select 1 from categories) then
        insert into categories (name)    
        values 
            ('Персональная съемка'), ('Репортаж'), ('Love story'), ('Свадебная съемка');
    end if;
end
$$;    

-- create materialized view if not exists home_page_view as
--     select
--         p.s3_file_path,
--         p.uploaded_at,
--         p.shoot_year
--     from photos p
--     where p.s3_file_path;