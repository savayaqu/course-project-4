<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\WarningResource;
use App\Models\User;
use App\Models\Warning;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class WarningController extends Controller
{
    public function create(Request $request, User $user): JsonResponse
    {
        $warning = Warning::create([
            'user_id' => $user->id,
            'comment' => $request->comment
        ]);
        // TODO: Блокировать пользователя при достижении максимального числа (по настройкам) предупреждений
        return response()->json(['warning' => WarningResource::make($warning)], 201);
    }

    public function destroy(User $user, Warning $warning): JsonResponse
    {
        $warning->delete();
        return response()->json(null, 204);
    }
}
