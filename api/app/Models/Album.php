<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\Auth;
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

    public static function getSign($album_id)
    {
        // Формируем строку user->token:album_id
        $data = Auth::user()->getRememberToken() . ':' . $album_id;
        // Проверяем, есть ли токен уже в кэше
        $cachedToken = Cache::get("token_for_{$data}");

        if ($cachedToken) {
            return $cachedToken; // Возвращаем существующий токен, если он уже есть
        }

        // Создаем новый токен и шифруем данные
        $token = Hash::make($data);

        // Сохраняем токен в кэше на 6 часов под ключом "token_for_user_token:album_id"
        Cache::put("token_for_{$data}", $token, now()->addHours(6));

        return $token;
    }

    public static function checkSign($token, $album_id): bool
    {
        try {
            // Расшифровываем токен для извлечения данных
            $decryptedData =  Auth::user()->getRememberToken() . ':' . $album_id;
            // Проверяем, есть ли расшифрованные данные в кэше под этим ключом
            $cachedToken = Cache::get("token_for_{$decryptedData}");

            // Сравниваем исходный токен с тем, что хранится в кэше
            if ($cachedToken != $token) {
                return false;
            }
            return true;


        } catch (\Exception $e) {
            // Если произошла ошибка (например, токен недействителен), возвращаем false
            return false;
        }
    }




}
