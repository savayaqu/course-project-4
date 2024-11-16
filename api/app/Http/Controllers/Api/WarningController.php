<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\WarningResource;
use App\Models\User;
use App\Models\Warning;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Storage;

class WarningController extends Controller
{
    public function create(Request $request, User $user): JsonResponse
    {
        $settings = json_decode(Storage::get('settings.json'), true); // TODO: поменять
        $warning = Warning::create([
            'user_id' => $user->id,
            'comment' => $request->comment
        ]);
        if(Warning::where('user_id', $user->id)->count() >= $settings['warning_limit_for_ban']) // TODO: поменять
        {
            $user->update(['is_banned' => true]);
        }
        return response()->json(['warning' => WarningResource::make($warning)], 201);
    }

    public function destroy(User $user, Warning $warning): JsonResponse
    {
        $warning->delete();
        return response()->json(null, 204);
    }
}
