<?php

namespace App\Http\Requests\Api\Complaint;

use App\Http\Requests\Api\ApiRequest;

class ComplaintUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'status' => 'required|integer|between:0,1',
        ];
    }
}
