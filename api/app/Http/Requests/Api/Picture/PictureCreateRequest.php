<?php

namespace App\Http\Requests\Api\Picture;

use App\Http\Requests\Api\ApiRequest;

class PictureCreateRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'pictures' => 'required|array|min:1',
            //'pictures.*' => 'required|file|mimes:jpeg,jpg,png,gif'
        ];
    }
}
