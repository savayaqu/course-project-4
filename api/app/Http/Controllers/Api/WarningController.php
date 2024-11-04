<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\User;
use App\Models\Warning;
use Illuminate\Http\Request;

class WarningController extends Controller
{
    public function create(Request $request, $userId)
    {
        User::findOrFail($userId);
        $warning = Warning::create([
           'user_id' => $userId,
           'comment' => $request->comment
        ]);
        return response()->json($warning)->setStatusCode(201);
    }
    public function destroy($userId, $warningId)
    {
        User::findOrFail($userId);
        Warning::findOrFail($warningId)->delete();
        return response()->json()->setStatusCode(204);

    }
}
