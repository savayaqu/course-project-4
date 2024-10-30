<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Crypt;
use Illuminate\Support\Facades\Hash;

class Album extends Model
{
    protected $fillable = [
      'name',
      'path',
      'user_id'
    ];
    public function user() {
        return $this->belongsTo(User::class);
    }
    public function pictures()
    {
        return $this->hasMany(Picture::class);
    }
    public function albumAccesses()
    {
        return $this->hasMany(AlbumAccess::class);
    }
    public function invitations()
    {
        return $this->hasMany(Invitation::class);
    }
    public function complaints()
    {
        return $this->hasMany(Complaint::class);
    }

    public static function getSign($user_id, $album_id)
    {
        // Формируем строку user_id:album_id
        $data = $user_id . ':' . $album_id;

        // Шифруем данные
        $token = Crypt::encrypt($data);

        // Сохраняем токен в кэше на 6 часов
        Cache::put($token, $data, now()->addHours(6));

        return $token;
    }

    public static function checkSign($token): bool
    {
        try {
            // Проверяем, есть ли токен в кэше
            if (!Cache::has($token)) {
                return false;
            }

            // Расшифровываем токен
            $decryptedData = Crypt::decrypt($token);

            // Сравниваем расшифрованные данные с данными в кэше
            return Cache::get($token) === $decryptedData;

        } catch (\Exception $e) {
            // Ошибка расшифровки или токен не найден в кэше
            return false;
        }
    }


}
