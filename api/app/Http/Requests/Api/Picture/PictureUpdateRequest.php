<?php

namespace App\Http\Requests\Api\Picture;

use App\Http\Requests\Api\ApiRequest;

class PictureUpdateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name' => [
                'required',
                'regex:~^[^/\\\\:*?"<>|]+$~u'
            ],
        ];
    }
}
