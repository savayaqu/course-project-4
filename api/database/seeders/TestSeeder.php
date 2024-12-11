<?php

namespace Database\Seeders;

use App\Models\Album;
use App\Models\AlbumAccess;
use App\Models\Complaint;
use App\Models\ComplaintType;
use App\Models\Picture;
use App\Models\Role;
use App\Models\User;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Facades\File;
use Illuminate\Database\Seeder;

class TestSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $roleAdminId = Role::firstOrCreate(['code' => 'admin'])->id;
        $roleUserId  = Role::firstOrCreate(['code' => 'user' ])->id;
        User::firstOrCreate(['login' => 'admin'],['name' => 'Админ', 'login' => 'admin', 'password' => 'Admin123!', 'is_banned' => 0, 'role_id' => $roleAdminId]);
        User::firstOrCreate(['login' => 'test1'],['name' => 'Чел 1', 'login' => 'test1', 'password' => 'Test123!', 'is_banned' => 0, 'role_id' => $roleUserId]);
        User::firstOrCreate(['login' => 'test2'],['name' => 'Чел 2', 'login' => 'test2', 'password' => 'Test123!', 'is_banned' => 0, 'role_id' => $roleUserId]);
        User::firstOrCreate(['login' => 'test3'],['name' => 'Чел 3', 'login' => 'test3', 'password' => 'Test123!', 'is_banned' => 0, 'role_id' => $roleUserId]);
        Album::firstOrCreate(['name' => 'Яблоко'],['name' =>"Яблоко", 'path' => fake()->slug(), 'user_id' => User::inRandomOrder()->whereNot('role_id', $roleAdminId)->first()->id]);
        Album::firstOrCreate(['name' => 'Помидор'],['name'=>"Помидор", 'path' => fake()->slug(), 'user_id' => User::inRandomOrder()->whereNot('role_id', $roleAdminId)->first()->id]);
        Album::firstOrCreate(['name' => 'Огурец'],['name' =>"Огурец", 'path' => fake()->slug(), 'user_id' => User::inRandomOrder()->whereNot('role_id', $roleAdminId)->first()->id]);
        AlbumAccess::firstOrCreate(['album_id' => 1, 'user_id' => 3]);
        AlbumAccess::firstOrCreate(['album_id' => 2, 'user_id' => 4]);
        AlbumAccess::firstOrCreate(['album_id' => 1, 'user_id' => 4]);
        AlbumAccess::firstOrCreate(['album_id' => 3, 'user_id' => 2]);
        ComplaintType::firstOrCreate(['name' => 'Террористическая пропаганда']);
        ComplaintType::firstOrCreate(['name' => 'Сцены насилия']);
        ComplaintType::firstOrCreate(['name' => 'Разжигание ненависти']);
        ComplaintType::firstOrCreate(['name' => 'Пропаганда насилия']);
        ComplaintType::firstOrCreate(['name' => 'Нарушение авторских прав']);
        ComplaintType::firstOrCreate(['name' => 'Шок-контент']);
        // Путь к существующим картинкам
        $parameter = $this->command->ask('Введите абсолютный путь до папки с картинками', Storage::path('sample'));
        $sourceDirectory = $parameter;
            // Получение всех файлов из директории
            $files = File::files($sourceDirectory);

            foreach ($files as $file) {
                // Ищем случайного пользователя и альбом
                $album = Album::inRandomOrder()->first();
                $user = $album->user;
                // Путь назначения
                $destinationDirectory = $album->getPath();

                // Создаем директорию назначения, если её нет
                if (!Storage::exists($destinationDirectory)) {
                    Storage::makeDirectory($destinationDirectory);
                }

                // Имя файла
                $fileName = $file->getFilename();
                $filePath = Storage::path("$destinationDirectory/$fileName");

                // Копируем файл
                File::copy($file->getRealPath(), $filePath);

                // Добавляем запись в базу данных
                DB::table('pictures')->insertOrIgnore([
                    'name' => $fileName,
                    'hash' => md5_file($filePath),
                    'date' => now(),
                    'size' => File::size($filePath),
                    'width' => getimagesize($filePath)[0] ?? 0, // Проверка на null для ширины
                    'height' => getimagesize($filePath)[1] ?? 0, // Проверка на null для высоты
                    'album_id' => $album->id,
                    'created_at' => now(),
                    'updated_at' => now(),
                ]);
            }

        for($i=0;$i<=60;$i++)
        {
            // Получение случайного альбома пользователя
            $album = Album::inRandomOrder()->first();
            $user = $album->user;
            // Получение случайной картинки из альбома
            $picture = Picture::where('album_id', $album->id)->inRandomOrder()->first();
            if($picture)
            {
                // Создание жалобы
                DB::table('complaints')->insertOrIgnore([
                    'description' => 'This is a test complaint.',
                    'complaint_type_id' => ComplaintType::inRandomOrder()->first()->id,
                    'picture_id' => $picture->id,
                    'album_id' => $album->id,
                    'about_user_id' => $user->id,
                    'from_user_id' => User::where('role_id', $roleUserId)->inRandomOrder()->first()->id, // случайный пользователь, кроме создателя,
                    'status' => null,
                ]);
            }
        }

    }
}
