<?php

namespace App\Http\Requests\Api\Tag;

use App\Http\Requests\Api\ApiRequest;
use App\Models\Tag;
use Illuminate\Validation\Rule;

class TagUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        $tag = $this->route('tag');
        if ($tag instanceof Tag) $tag = $tag->id;
        return [
            'value' => [
                'string',
                'min:1',
                'max:255',
                Rule::unique(Tag::class, 'value')
                    ->where('user_id', auth()->id())
                    ->ignore((int)$tag),
            ],
        ];
    }
}
