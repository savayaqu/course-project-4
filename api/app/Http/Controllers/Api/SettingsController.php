<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Models\ComplaintType;
use Illuminate\Support\Facades\Artisan;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\File;
use Illuminate\Support\Facades\Storage;
use Illuminate\Http\Request;

class SettingsController extends Controller
{
    public function public(Request $request)
    {
        $settings = Cache::remember('public_settings', now()->addMinutes(10), function () {

            return  [
               'max_free_storage_size' => config('settings.storage_size') . ' byte',
               'allowed_preview_sizes' => config('settings.allowed_sizes'),
               'allowed_image_mimes' => config('settings.allowed_mimes'),
                'complaint_types' => ComplaintType::all()

            ];

        });
        // если авторизован то выдать состояние памяти?
        if($user = Auth::user())
        {
            $settings['max_free_storage_size'] = [
                'current_storage_size' => $user_storage = $user->pictures()->sum('size'),
                'max_free_storage_size' => $free_storage = config('settings.storage_size') . ' byte',
                'percentage_used' => $free_storage > 0 ? round(($user_storage / $free_storage) * 100, 2) : 0,
                'storage_status' => $free_storage > 0 && $user_storage >= $free_storage ? 'exceeded' : 'available',
            ];
        }
        return response()->json($settings)->header('Cache-Control', 'public, max-age=' . (30 * 24 * 60 * 60)); // 30 дней
    }
    // Получение всех настроек
    public function index()
    {
        $settings = config('settings');
        return response()->json($settings);
    }
    public function edit(Request $request)
    {
        // Валидация входных данных
        $validated = $request->validate([
            'type' => 'required|string', //allowed_mime, allowed_sizes, warning_limit_for_ban, storage_size
            'value' => 'required|string', // Значение, которое будет записано (строка)
        ]);

        $type = $validated['type'];
        $value = $validated['value']; // Принимаем строку

        // Путь к .env файлу
        $envPath = base_path('.env');

        // Проверяем, существует ли .env файл
        if (File::exists($envPath)) {
            // Читаем содержимое .env файла
            $envContent = File::get($envPath);

            // Создаем ключ для поиска
            $key = strtoupper($type);

            // Проверяем, существует ли уже эта настройка в .env
            $pattern = "/^{$key}=(.*)$/m";

            // Если строка найдена, заменяем её
            if (preg_match($pattern, $envContent)) {
                // Заменяем старое значение на новое
                $envContent = preg_replace($pattern, "{$key}={$value}", $envContent);
            } else {
                // Если строки нет, добавляем её в конец
                $envContent .= "\n{$key}={$value}";
            }

            // Записываем обновленное содержимое в .env файл
            File::put($envPath, $envContent);

            // Очистка кэша конфигурации, чтобы новые значения вступили в силу
            Artisan::call('config:clear');

            return response()->json(['message' => 'Settings updated successfully']);
        }

        return response()->json(['error' => 'Unable to update .env file'], 500);
    }

}
