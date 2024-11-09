<?php

namespace App\Http\Requests\Api\User;

use App\Http\Requests\Api\ApiRequest;

class UserSelfUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name'     => 'string|min:2|max:255',
            'login'    => 'string|min:2|max:64|regex:/^[a-zA-Z0-9_-]+$/',
            'password' => 'string|min:8|max:255',
        ];
    }
}
