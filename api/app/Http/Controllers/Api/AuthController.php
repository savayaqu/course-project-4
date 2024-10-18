<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\Api\ApiException;
use App\Http\Controllers\Controller;
use App\Http\Requests\Api\RegisterRequest;
use App\Models\Role;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class AuthController extends Controller
{
    public function login(Request $request)
    {
        if (!Auth::attempt($request->only('login', 'password'))) {
            throw new ApiException(401, 'Invalid credentials');
        }

        $user = Auth::user();

        $token = $user->createToken('remember_token')->plainTextToken;

        $user->remember_token = $token;
        $user->save();

        return response([
            'token' => $token,
            'data' => $user,
        ], 200);
    }

    public function register(RegisterRequest $request)
    {
        $role_id = Role::where(['code' =>'user'])->first()->id;
        $user = User::create([...$request->validated(), 'role_id' => $role_id]);
        $token = $user->createToken('remember_token')->plainTextToken;
        $user->remember_token = $token;
        $user->save();
        return response()->json([$user, 'token' => $token])->setStatusCode(201);

    }
    public function logout(Request $request)
    {
        $request->user()->currentAccessToken()->delete();
        $user = $request->user();
        $user->remember_token = null;
        $user->save();

        return response()->json([
            'message' => 'Logged out successfully',
        ])->setStatusCode(200);
    }


}
