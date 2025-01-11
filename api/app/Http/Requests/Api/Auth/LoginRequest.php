<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\Api\ApiRequest;
use Illuminate\Support\Str;

class LoginRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'login'    => 'required|string',
            'password' => 'required|string',
        ];
    }
    protected function prepareForValidation()
    {
        try {
            if (is_string(strval($this->login))) {
                $this->merge([
                    'login' => Str::lower($this->login),
                ]);
            }
        }
        catch (\Exception) {}
    }
}
