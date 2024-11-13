<?php

namespace App\Exceptions\Api;

use Illuminate\Http\Exceptions\HttpResponseException;


class AlreadyExistsException extends HttpResponseException
{
    public function __construct($model)
    {
        $name = class_basename($model);
        $message = ucfirst(($model ?  "$name " : '') . 'already exists');
        parent::__construct(throw new ApiException($message, 409, data: [$name => $model]));
    }
}
