﻿import React, { Component } from 'react';
import { connect } from 'react-redux';
import CategoryList from '../components/category/category-list';
import Spinner from '../components/spinner';
import get_categories from '../actions/category-list';


class CategoryListWrapper extends Component {

    componentDidMount = () => this.props.get_categories();

    render() {

        const { data, isPending, isError } = this.props;
        // const hasData = !(isPending || isError);

        // const errorMessage = isError ? <ErrorIndicator/> : null;
        const spinner = isPending ? <Spinner /> : null;
        const content = !isPending ? <CategoryList data_list={data} /> : null;

        return <>
            {spinner}
            {content}
        </>
    }
}

const mapStateToProps = (state) => (state.categories);

const mapDispatchToProps = (dispatch) => {
    return {
        get_categories: () => dispatch(get_categories())
    }
};

export default connect(mapStateToProps, mapDispatchToProps)(CategoryListWrapper);