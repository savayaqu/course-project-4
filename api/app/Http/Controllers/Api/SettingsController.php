<?php

namespace App\Http\Controllers\Api;

use App\Cacheables\SpaceInfo;
use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Settings\SettingsUpdateRequest;
use App\Http\Resources\ComplaintTypeResource;
use App\Models\ComplaintType;
use Illuminate\Support\Facades\Artisan;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\File;

class SettingsController extends Controller
{
    public function public()
    {
        $settings = Cache::remember('public_settings', (7 * 24 * 60 * 60), fn () => [
            'allowed_upload_mimes'  => config('settings.allowed_upload_mimes'),
            'allowed_preview_sizes' => config('settings.allowed_preview_sizes'),
            'warning_limit_for_ban' => config('settings.warning_limit_for_ban'),
            'free_storage_limit'    => config('settings.free_storage_limit'),
            'complaint_types' => ComplaintTypeResource::collection(ComplaintType::all()),
        ]);

        $spaceInfo = SpaceInfo::getCached();
        return response()->json([
            'settings' => [
                ...$settings,
                'is_upload_disabled' => $spaceInfo->usedPercent >= config('settings.upload_disable_percentage'),
            ],
        ]);
    }

    public function index()
    {
        $settings = config('settings');
        $space = SpaceInfo::getCached();

        return response()->json([
            'settings' => $settings,
            'space' => $space
        ]);
    }

    public function edit(SettingsUpdateRequest $request)
    {
        Cache::forget('public_settings');

        $key   = $request->key;
        $value = $request->value; // Принимаем строку

        // Путь к .env файлу
        $envPath = base_path('.env');

        // Проверяем, существует ли .env файл
        if (!File::exists($envPath))
            throw new ApiException('Unable to update .env file', 500);

        // Читаем содержимое .env файла
        $envContent = File::get($envPath);

        // Ключ для поиска
        $key = strtoupper($key);

        // Проверяем, существует ли уже эта настройка в .env
        $pattern = "/^{$key}=(.*)$/m";

        // Если строка найдена, заменяем её
        if (preg_match($pattern, $envContent))
            $envContent = preg_replace($pattern, "{$key}={$value}", $envContent);

        // Если строки нет, добавляем её в конец
        else
            $envContent .= "\n{$key}={$value}";

        // Записываем обновленное содержимое в .env файл
        File::put($envPath, $envContent);

        // Очистка кэша конфигурации, чтобы новые значения вступили в силу
        Artisan::call('config:clear');

        return response()->json(['message' => 'Settings updated successfully']);
    }
}
