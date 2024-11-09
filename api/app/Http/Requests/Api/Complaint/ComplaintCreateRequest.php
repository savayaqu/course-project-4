<?php

namespace App\Http\Requests\Api\Complaint;

use App\Http\Requests\Api\ApiRequest;
class ComplaintCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'description' => 'required|string|min:1|max:255',
            'type_id'     => 'required|integer|exists:complaint_types,id',
        ];
    }
}
