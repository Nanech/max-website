-- take extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";


-- categories of albums
create table if not exists categories (
    id SMALLSERIAL PRIMARY key,
    name text not null unique
);

-- albums which group photos
create table if not exists albums (
    id uuid primary key default uuid_generate_v4(),
    name text not null,
    created_at timestamp with time zone default current_timestamp,
    shoot_year smallint not null,
    category_id smallint references categories(id) on delete set null,
    viability_status text not null default 'private'
);

-- main storage for photos 
create table if not exists photos (
    id uuid primary key default uuid_generate_v4(),
    album_id uuid not null references albums(id) on delete cascade,
    uploaded_at timestamp with time zone default current_timestamp
);


-- default values in categories
do $$
begin 
    if not exists (select 1 from categories) then
        insert into categories (name)    
        values 
            ('Персональная съемка'), ('Репортаж'), ('Love story'), ('Свадебная съемка');
    end if;
end
$$;    
