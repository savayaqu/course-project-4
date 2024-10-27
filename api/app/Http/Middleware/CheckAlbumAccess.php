<?php

namespace App\Http\Middleware;

use App\Exceptions\Api\ApiException;
use App\Models\Album;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Log;
use Symfony\Component\HttpFoundation\Response;

class CheckAlbumAccess
{

    public function handle(Request $request, Closure $next)
    {
        $user = Auth::user();
        $album_id = $request->route('album');

        // Получаем оригинальный хэш альбома из базы данных
        $albumHash = Album::where('user_id', $user->id)->where('id', $album_id)->value('hash');

        if (!$albumHash) {
            Log::warning('Альбом не найден или доступ запрещён', ['album_id' => $album_id]);
            throw new ApiException('Альбом не найден или доступ запрещён', 404);
        }

        $providedKey = $request->query('access_key');
        list($userIdFromKey, $providedSignCode) = explode('_', $providedKey);

        if ($userIdFromKey != $user->id) {
            Log::warning('Неверный идентификатор пользователя', ['userIdFromKey' => $userIdFromKey, 'user_id' => $user->id]);
            throw new ApiException('Доступ запрещён: Неверный идентификатор пользователя', 403);
        }

        // Используем оригинальный хэш альбома для формирования ключа кеша
        $cacheKey = "keyAccess:to=$albumHash;for=$user->id";
        $expectedSignCode = Cache::get($cacheKey);

        Log::info('Проверка кеша', ['cacheKey' => $cacheKey, 'expectedSignCode' => $expectedSignCode]);

        if (!$expectedSignCode) {
            Log::warning('Ключ доступа не найден в кеше', ['cacheKey' => $cacheKey]);
            throw new ApiException('Доступ запрещён: Ключ доступа не найден', 403);
        }

        // Проверка хэша с использованием Hash::check
        if (!Hash::check($cacheKey, $providedSignCode)) {
            Log::warning('Неверный ключ доступа', ['providedSignCode' => $providedSignCode, 'cacheKey' => $cacheKey]);
            throw new ApiException('Доступ запрещён: Неверный ключ доступа', 403);
        }

        return $next($request);
    }



}
