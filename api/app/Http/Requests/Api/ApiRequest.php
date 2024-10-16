<?php

namespace App\Http\Requests\Api;

use App\Exceptions\Api\ApiException;
use Illuminate\Contracts\Validation\Validator;
use Illuminate\Foundation\Http\FormRequest;

class ApiRequest extends FormRequest
{
    protected function failedValidation(Validator $validator)
    {
        throw new ApiException('Validation failed',422, $validator->errors());
    }
    protected function failedAuthorization(){
        return response('ошибка доступа чтоль', 403);
    }
}
