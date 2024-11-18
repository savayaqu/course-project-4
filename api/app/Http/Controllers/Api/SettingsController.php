<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use Illuminate\Support\Facades\Storage;

class SettingsController extends Controller
{
    private $filePath = 'settings.json';

    // Получение всех настроек
    public function index()
    {
        if(!Storage::exists($this->filePath))
        {
            throw new ApiException('Settings file not found', 404);
        }
        $settings = json_decode(Storage::get($this->filePath), true);
        return response()->json($settings);
    }
}
