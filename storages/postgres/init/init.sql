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
    visibility_status text not null default 'private'    
);

-- main storage for photos 
create table if not exists photos (
    id uuid primary key default uuid_generate_v4(),
    album_id uuid not null references albums(id) on delete cascade,
    uploaded_at timestamp with time zone default current_timestamp
);

alter table albums add column if not exists cover_photo_id uuid references photos(id) on delete set null;

create table if not exists showcase_pages (
    id serial primary key,
    name text not null,
    page_key text not null unique
);

create table if not exists showcase_section_types (
    id serial primary key,
    name text not null unique,
    removable boolean not null default true
);

create table if not exists showcase_sections (
    id serial primary key,
    page_id int not null references showcase_pages(id) on delete cascade,
    name text not null,
    section_key text not null,
    showcase_section_type_id int not null references showcase_section_types(id) on delete restrict,
    display_order int not null default 0,
    
    constraint udx_showcase_section_key unique(page_id, section_key)    
);

create table if not exists showcase_items (
    id serial primary key,
    section_id int not null references showcase_sections(id) on delete cascade,
    item_type text not null check(item_type in ('album', 'photo')),       
    album_id uuid references albums(id) on delete cascade,
    photo_id uuid references photos(id) on delete cascade,
    created_at timestamp with time zone default current_timestamp,
    display_order int not null default 0,
    
    constraint chk_showcase_target check(
        (item_type = 'album' and album_id is not null and photo_id is null) or
        (item_type = 'photo' and photo_id is not null and album_id is null)
    )
);

----------------- data seders ------------------

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

-- default values in showcase_section_type
do $$
begin 
    if not exists (select 1 from showcase_section_types) then
        insert into showcase_section_types (name, removable)    
        values 
            ('hero', false), ('album_list', false), ('photo-list', false);
    end if;
end
$$;

-- default pages in showcase_pages
do $$
begin 
    if not exists (select 1 from showcase_pages) then
        insert into showcase_pages (name, page_key)    
        values 
            ('Главная', 'home'), ('Прайс', 'price'), ('Портфолио', 'portfolio');
    end if;
end
$$;

-- default price page section
do $$
begin
    if not exists (select 1 from showcase_sections where page_id = 2) then
        insert into showcase_sections (page_id, name, section_key, showcase_section_type_id, display_order)    
        values 
            (2, 'Прайс', 'price-list', 3, 0);
    end if;
end 
$$;

-- default portfolio page section
do $$
begin 
    if not exists (select 1 from showcase_sections where page_id = 3) then
        insert into showcase_sections (page_id, name, section_key, showcase_section_type_id, display_order)    
        values 
            (3, 'Портфолио', 'portfolio-list', 3, 0);
    end if;
end 
$$;

-- default home page sections
do $$
begin 
    if not exists (select 1 from showcase_sections where page_id = 1) then
        insert into showcase_sections (page_id, name, section_key, showcase_section_type_id, display_order)    
        values 
            (1, 'Акцент фотография', 'hero', 1, 0),
            (1, 'Альбомы', 'album-list', 2, 1),
            (1, 'Фотография брони', 'booking-photo', 3, 2),
            (1, 'Фотограф', 'about-photographer', 3, 3);         
    end if;
end 
$$;