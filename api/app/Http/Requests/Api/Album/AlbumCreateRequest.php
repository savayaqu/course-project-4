<?php

namespace App\Http\Requests\Api\Album;

use App\Http\Requests\Api\ApiRequest;

class AlbumCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name' => 'required|string|max:255',
            'path' => 'nullable|string|max:255',
        ];
    }
}
