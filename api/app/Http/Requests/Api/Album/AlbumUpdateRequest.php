<?php

namespace App\Http\Requests\Api\Album;

use App\Http\Requests\Api\ApiRequest;
use App\Models\Album;
use Illuminate\Validation\Rule;

class AlbumUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        $album = $this->route('album');
        if ($album instanceof Album) $album = $album->id;
        return [
            'name' => [
                'string',
                'max:255',
                Rule::unique(Album::class, 'name')
                    ->where('user_id', auth()->id())
                    ->ignore($album),
            ],
            'path' => [
                'nullable',
                'string',
                'max:255',
                Rule::unique(Album::class, 'path')
                    ->where('user_id', auth()->id())
                    ->ignore($album),
            ],
        ];
    }
}
