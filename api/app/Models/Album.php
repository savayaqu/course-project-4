<?php

namespace App\Models;

use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Hash;

class Album extends Model
{
    protected $fillable = [
        'name',
        'path',
        'user_id',
    ];
    public static function getFolderSize(string $folderPath): int
    {
        // Получаем полный путь до директории в файловой системе
        $realPath = storage_path("app/{$folderPath}");

        // Проверяем, существует ли папка
        if (!is_dir($realPath)) {
            return 0;
        }

        // Выполняем команду du для получения размера директории
        $size = shell_exec("du -sb {$realPath} | cut -f1");

        // Убеждаемся, что результат преобразуется в число
        return (int) $size;
    }


    public static function getPathStatic($userId, $albumId): string {
        return "albums/$userId/$albumId";
    }
    public function getPath(): string {
        return $this->getPathStatic($this->user_id, $this->id);
    }

    public static function signNonHash(User $user, $albumId): string {
        $currentDay = date("Y-m-d");
        $userToken = $user->tokens[0]->token;
        return $userToken . $currentDay . $albumId;
    }
    public static function signCacheKey($albumId, $userId): string {
        return "signAccess:to=$albumId;for=$userId";
    }
    public function getSign(User $user): string {
        $cacheKey = static::signCacheKey($this->id, $user->id);
        $cachedSign = Cache::get($cacheKey);
        if ($cachedSign)
            return $user->id .'_'. $cachedSign;

        $string = static::signNonHash($user, $this->id);
        $signCode = base64_encode(Hash::make($string));

        Cache::put($cacheKey, $signCode . '_' . $this->user_id, 3600);
        return $user->id .'_'. $signCode;
    }

    public function user() {
        return $this->belongsTo(User::class);
    }
    public function usersViaAccess() {
        return $this->belongsToMany(User::class, 'album_accesses')->using(AlbumAccess::class);
    }
    public function pictures() {
        return $this->hasMany(Picture::class);
    }
    public function albumAccesses() {
        return $this->hasMany(AlbumAccess::class);
    }
    public function invitations() {
        return $this->hasMany(Invitation::class);
    }
    public function complaints() {
        return $this->hasMany(Complaint::class);
    }
}
