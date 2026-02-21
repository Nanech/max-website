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
    category_id smallint references categories(id) on delete set null
);

-- main storage for photos 
create table if not exists photos (
    id uuid primary key default uuid_generate_v4(),
    album_id uuid references albums(id) on delete cascade,
    s3_path text not null unique,
    uploaded_at timestamp with time zone default current_timestamp,
    photo_status smallint not null default 0
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
