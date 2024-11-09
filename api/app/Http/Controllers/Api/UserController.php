<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\User\UserSelfUpdateRequest;
use App\Http\Requests\Api\User\UserUpdateRequest;
use App\Models\User;
use Illuminate\Support\Facades\Auth;

class UserController extends Controller
{
    public function show($userId)
    {
        return response()->json(User::findOrFailCustom($userId), 200);
    }
    public function edit(UserUpdateRequest $request, $userId)
    {
        $data = array_filter($request->all(), function ($value) {
            return !is_null($value) && $value !== '';
        });

        User::findOrFailCustom($userId)->update($data);
        return response()->json(User::findOrFailCustom($userId), 200);
    }
    public function index()
    {
        return response()->json(User::all())->setStatusCode(200);
    }
    public function self()
    {
        return response()->json(Auth::user())->setStatusCode(200);
    }
    public function selfEdit(UserSelfUpdateRequest $request)
    {
        $data = array_filter($request->all(), function ($value) {
            return !is_null($value) && $value !== '';
        });
        Auth::user()->update($data);
        return response()->json(Auth::user())->setStatusCode(200);
    }
}
