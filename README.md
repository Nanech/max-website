# Про проект

Целью проекта является создание веб-сайта для моего друга фотографа, где я могу попрактиковаться в разработке веб-приложений и помочь другу продвигать его работы.

Хочу сделать упор на свое собственное обучение, чтобы улучшить навыки в разработке и понимании современных технологий. Поэтому этот проект можно считать как учебным.

## Про стек

- **Frontend**: Vue, JS, Tailwind CSS
- **Backend**: ASP.NET Core, C#
- **Хранение данных**: PostgreSQL (мета-информация) + Minio (для хранения изображений) + Redis (для кэша)

## TODO

I need to do MVP website

- Move to single bucket with prefix
- Validating upload images for more secure
- transaction for uploading file
- Path Traversal error
- Mime type validation
- Use transaction in Upload method
- Controller stack leaking
- Initialize retry policy
- Validate capacity of file
- Thumbnails original/large/image
- Multiply loading
