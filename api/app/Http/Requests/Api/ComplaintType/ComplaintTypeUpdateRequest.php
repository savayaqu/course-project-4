<?php

namespace App\Http\Requests\Api\ComplaintType;

use App\Http\Requests\Api\ApiRequest;

class ComplaintTypeUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name' => 'string|max:255',
        ];
    }
}
