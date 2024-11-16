<?php

namespace App\Http\Controllers;

use App\Exceptions\Api\ApiException;
use Illuminate\Http\Request;
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
