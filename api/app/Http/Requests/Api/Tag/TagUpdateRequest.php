<?php

namespace App\Http\Requests\Api\Tag;

use App\Http\Requests\Api\ApiRequest;
use Illuminate\Validation\Rule;

class TagUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'value' => [
                'string|min:1|max:255',
                Rule::unique('tags', 'value')->where('user_id', auth()->id()),
            ],
        ];
    }
}
