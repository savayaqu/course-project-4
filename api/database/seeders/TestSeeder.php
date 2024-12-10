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
        User::firstOrCreate(['name' => 'Админ', 'login' => 'admin', 'password' => 'Admin123!', 'is_banned' => 0, 'role_id' => $roleAdminId]);
        User::firstOrCreate(['name' => 'Чел 1', 'login' => 'test1', 'password' => 'Test123!', 'is_banned' => 0, 'role_id' => $roleUserId]);
        User::firstOrCreate(['name' => 'Чел 2', 'login' => 'test2', 'password' => 'Test123!', 'is_banned' => 0, 'role_id' => $roleUserId]);
        User::firstOrCreate(['name' => 'Чел 3', 'login' => 'test3', 'password' => 'Test123!', 'is_banned' => 0, 'role_id' => $roleUserId]);
        Album::firstOrCreate(['name' => 'Порно', 'path' => 'porno/love', 'user_id' => 2]);
        Album::firstOrCreate(['name' => 'Порно', 'path' => 'porno/love', 'user_id' => 3]);
        Album::firstOrCreate(['name' => 'Порно', 'path' => 'porno/love', 'user_id' => 4]);
        AlbumAccess::firstOrCreate(['album_id' => 1, 'user_id' => 3]);
        AlbumAccess::firstOrCreate(['album_id' => 2, 'user_id' => 4]);
        AlbumAccess::firstOrCreate(['album_id' => 1, 'user_id' => 4]);
        AlbumAccess::firstOrCreate(['album_id' => 3, 'user_id' => 2]);
        ComplaintType::firstOrCreate(['name' => 'Детская порнография']);
        ComplaintType::firstOrCreate(['name' => 'Расчленёнка']);
        // Путь к существующим картинкам
        $sourceDirectory = 'testnorm';

        // Получение всех файлов из директории
        $files = Storage::files($sourceDirectory);
        foreach ($files as $file) {
            // Ищем случайного пользователя и альбом
            $user = User::inRandomOrder()->whereNot('role_id', $roleAdminId)->first();
            $album = Album::where('user_id', $user->id)->inRandomOrder()->first();

            // Путь назначения
            $destinationDirectory = $album->getPath();

            // Создаем директорию назначения, если её нет
            if (!Storage::exists($destinationDirectory)) {
                Storage::makeDirectory($destinationDirectory);
            }

            // Имя файла
            $fileName = basename($file);

            // Копируем файл
            Storage::copy($file, $destinationDirectory . '/' . $fileName);

            // Добавляем запись в базу данных
            DB::table('pictures')->insertOrIgnore([
                'name' => $fileName,
                'hash' => md5_file(Storage::path($destinationDirectory . '/' . $fileName)),
                'date' => now(),
                'size' => Storage::size($destinationDirectory . '/' . $fileName),
                'width' => getimagesize(Storage::path($destinationDirectory . '/' . $fileName))[0],
                'height' => getimagesize(Storage::path($destinationDirectory . '/' . $fileName))[1],
                'album_id' => $album->id,
                'created_at' => now(),
                'updated_at' => now(),
            ]);
        }
        for($i=0;$i<=3;$i++)
        {
            // Получение случайного пользователя
            $userId = User::inRandomOrder()->first()->id;

            // Получение случайного альбома пользователя
            $albumId = Album::where('user_id', $userId)->inRandomOrder()->first()->id;

            // Получение случайной картинки из альбома
            $pictureId = Picture::where('album_id', $albumId)->inRandomOrder()->first()->id;

            // Создание жалобы
            Complaint::create([
                'description' => 'This is a test complaint.',
                'complaint_type_id' => ComplaintType::inRandomOrder()->first()->id,
                'picture_id' => $pictureId,
                'album_id' => $albumId,
                'about_user_id' => $userId,
                'from_user_id' => User::where('id', '!=', $userId)->inRandomOrder()->first()->id, // случайный пользователь, кроме создателя,
                'status' => null,
            ]);
        }
    }
}
