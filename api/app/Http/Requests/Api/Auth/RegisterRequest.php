<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\Api\ApiRequest;

class RegisterRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name' => 'required|string|min:2|max:255|unique:users',
            'login' => 'required|string|min:2|max:64|regex:/^[a-zA-Z0-9_-]+$/|unique:users',
            'password' => 'required|string|min:8|max:255',
        ];
    }
}
