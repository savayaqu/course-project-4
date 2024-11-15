<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Resources\WarningResource;
use App\Models\User;
use App\Models\Warning;
use Illuminate\Http\Request;

class WarningController extends Controller
{
    public function create(Request $request, User $user)
    {
        $warning = Warning::create([
            'user_id' => $user->id,
            'comment' => $request->comment
        ]);
        // TODO: Блокировать пользователя при достижении максимального числа (по настройкам) предупреждений
        return response(['warning' => WarningResource::make($warning)], 201);
    }

    public function destroy(User $user, Warning $warning)
    {
        $warning->delete();
        return response(null, 204);
    }
}
