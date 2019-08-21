import * as React from 'react';
import Login from './Login';
import { RouteComponentProps } from 'react-router';
import { Typography } from '@material-ui/core';
import { ISelfUser } from '../entities/user/ISelfUser';

export interface HomeProps extends RouteComponentProps {
    user?: ISelfUser;
}

export default function Home(props: HomeProps) {

    if (!props.user)
        return <Login />;
    else {
        return <Typography>Hi, {props.user.user.username}.</Typography>;
    }
}