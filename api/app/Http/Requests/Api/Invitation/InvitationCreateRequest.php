<?php

namespace App\Http\Requests\Api\Invitation;

use App\Http\Requests\Api\ApiRequest;
use Carbon\Carbon;

class InvitationCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'expires_at_integer' => 'nullable|integer|min:1|required_without:expires_at_date',
            'expires_at_date' => 'nullable|date|after:now|required_without:expires_at_integer',
            'joinLimit' => 'nullable|integer|min:1',
        ];
    }
}
