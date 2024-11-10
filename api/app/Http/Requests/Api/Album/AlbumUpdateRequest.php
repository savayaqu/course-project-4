<?php

namespace App\Http\Requests\Api\Album;

use App\Http\Requests\Api\ApiRequest;
use Illuminate\Foundation\Http\FormRequest;

class AlbumUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name' => 'string|max:255',
            'path' => 'nullable|string|max:255',
        ];
    }
}
