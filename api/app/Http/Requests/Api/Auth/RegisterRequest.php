<?php

namespace App\Http\Requests\Api\Auth;

use App\Http\Requests\Api\ApiRequest;
use Illuminate\Support\Str;

class RegisterRequest extends ApiRequest
{
    public function rules(): array
    {
        return [
            'name'     => 'required|string|min:2|max:255',
            'login'    => 'required|string|min:2|max:64|regex:/^[a-zA-Z0-9_-]+$/|unique:users',
            'password' => 'required|string|min:8|max:255',
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
