<?php

namespace App\Http\Requests\Api\Warning;

use App\Http\Requests\Api\ApiRequest;
class WarningCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'comment' => 'required',
        ];
    }
}
