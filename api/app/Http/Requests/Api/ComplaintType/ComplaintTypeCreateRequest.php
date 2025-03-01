<?php

namespace App\Http\Requests\Api\ComplaintType;

use App\Http\Requests\Api\ApiRequest;

class ComplaintTypeCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
           'name' => 'required|string|max:255',
        ];
    }
}
