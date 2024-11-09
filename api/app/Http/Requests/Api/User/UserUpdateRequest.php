<?php

namespace App\Http\Requests\Api\User;

use App\Http\Requests\Api\ApiRequest;

class UserUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'role_id' => 'integer|exists:roles,id',
            'is_banned' => 'boolean'
        ];
    }
}
