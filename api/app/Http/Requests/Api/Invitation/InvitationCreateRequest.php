<?php

namespace App\Http\Requests\Api\Invitation;

use App\Http\Requests\Api\ApiRequest;

class InvitationCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'expires_at' => 'nullable|date|after:now',
            'timeLimit'  => 'nullable|integer|min:1',
            'joinLimit'  => 'nullable|integer|min:1',
        ];
    }
}
